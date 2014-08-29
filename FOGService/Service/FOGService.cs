﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Net;

using FOG;

namespace FOG{
	/// <summary>
	/// Coordinate all system wide FOG modules
	/// </summary>
	public partial class FOGService  : ServiceBase {
		//Define variables
		private Thread threadManager;
		private Thread notificationPipeThread;
		
		private List<AbstractModule> modules;
		private Status status;
		private int sleepDefaultTime = 60;
		private PipeServer notificationPipe;
		
		private const String LOG_NAME = "Service";
		
		//Module status -- used for stopping/starting
		public enum Status {
			Broken = 2,
			Running = 1,
			Stopped = 0
		}
		
		public FOGService() {
			//Initialize everything
			if(CommunicationHandler.getAndSetServerAddress()) {

				initializeModules();
				this.threadManager = new Thread(new ThreadStart(serviceLooper));
				this.status = Status.Stopped;
				
				//Setup the piper server
				this.notificationPipeThread = new Thread(new ThreadStart(notificationPipeHandler));
				this.notificationPipe = new PipeServer("fog_pipe");
				this.notificationPipe.MessageReceived += new PipeServer.MessageReceivedHandler(notificationPipeServer_MessageReceived);
			}
		}
		
		//This is run by the pipe thread, it will send out notifications to the tray
		private void notificationPipeHandler() {
			while (true) {
				if(!this.notificationPipe.isRunning()) 
					this.notificationPipe.start();			
				
				
				if(NotificationHandler.getNotifications().Count > 0) {
					//Split up the notification into 3 messages: Title, Message, and Duration
					this.notificationPipe.sendMessage("TLE:" + NotificationHandler.getNotifications()[0].getTitle());
					Thread.Sleep(750);
					this.notificationPipe.sendMessage("MSG:" + NotificationHandler.getNotifications()[0].getMessage());
					Thread.Sleep(750);
					this.notificationPipe.sendMessage("DUR:" + NotificationHandler.getNotifications()[0].getDuration().ToString());
					NotificationHandler.removeNotification(0);
				} 
				
				Thread.Sleep(3000);
			}

		}
		
		//Handle recieving a message
		private void notificationPipeServer_MessageReceived(Client client, String message) {
			LogHandler.log("PipeServer", "Message recieved");
			LogHandler.log("PipeServer",message);
		}	

		//Called when the service starts
		protected override void OnStart(string[] args) {
			if(!this.status.Equals(Status.Broken)) {
				this.status = Status.Running;
				
				//Start the pipe server
				this.notificationPipeThread.Priority = ThreadPriority.Normal;
				this.notificationPipeThread.Start();
			
				
				//Start the main thread that handles all modules
				this.threadManager.Priority = ThreadPriority.Normal;
				this.threadManager.IsBackground = true;
				this.threadManager.Name = "FOGService";
				this.threadManager.Start();
			}
        }
		
		//Load all of the modules
		private void initializeModules() {
			this.modules = new List<AbstractModule>();
			this.modules.Add(new ClientUpdater());
			this.modules.Add(new TaskReboot());
			this.modules.Add(new HostnameChanger());
			this.modules.Add(new DisplayManager());
			this.modules.Add(new SnapinClient());
			
		}
		
		//Called when the service stops
		protected override void OnStop() {
			if(!this.status.Equals(Status.Broken))
				this.status = Status.Stopped;
		}
		
		//Run each service
		private void serviceLooper() {
			//Only run the service if there wasn't a stop or shutdown request
			while (status.Equals(Status.Running) && !ShutdownHandler.isShutdownPending()) {
				foreach(AbstractModule module in modules) {
					if(ShutdownHandler.isShutdownPending() )
						break;
					
					//Log file formatting
					LogHandler.newLine();
					LogHandler.newLine();
					LogHandler.divider();
					
					try {
						module.start();
					} catch (Exception ex) {
						LogHandler.log(LOG_NAME, "Failed to start " + module.getName());
						LogHandler.log(LOG_NAME, "ERROR: " + ex.Message);
					}
					
					//Log file formatting
					LogHandler.divider();
					LogHandler.newLine();
					
				}
					
				//Once all modules have been run, sleep for the set time
				int sleepTime = getSleepTime();
				LogHandler.log(LOG_NAME, "Sleeping for " + sleepTime.ToString() + " seconds");
				Thread.Sleep(sleepTime * 1000);
			}
		}
		
		
		//Get the time to sleep from the FOG server, if it cannot it will use the default time
		private int getSleepTime() {
			LogHandler.log(LOG_NAME, "Getting sleep duration...");
			
			Response sleepResponse = CommunicationHandler.getResponse("/service/servicemodule-active.php");
			
			try {
				if(!sleepResponse.wasError() && !sleepResponse.getField("#sleep").Equals("")) {
					int sleepTime = int.Parse(sleepResponse.getField("#sleep"));
					if(sleepTime >= this.sleepDefaultTime) {
						return sleepTime;
					} else {
						LogHandler.log(LOG_NAME, "Sleep time set on the server is below the minimum of " + this.sleepDefaultTime.ToString());
					}
				}
			} catch (Exception ex) {
				LogHandler.log(LOG_NAME,"Failed to parse sleep time");
				LogHandler.log(LOG_NAME,"ERROR: " + ex.Message);				
			}
			
			LogHandler.log(LOG_NAME,"Using default sleep time");	
			
			return this.sleepDefaultTime;			
		}

	}
}