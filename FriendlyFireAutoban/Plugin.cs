using System;
using EXILED;

namespace SamplePlugin
{
	public class Plugin : EXILED.Plugin
	{
		//Instance variable for eventhandlers
		public EventHandlers EventHandlers;
		
		public override void OnEnable()
		{
			try
			{
				Debug("Initializing event handlers..");
				//Set instance varible to a new instance, this should be nulled again in OnDisable
				EventHandlers = new EventHandlers(this);
				//Hook the events you will be using in the plugin. You should hook all events you will be using here, all events should be unhooked in OnDisabled 
				Events.RoundStartEvent += EventHandlers.OnRoundStart;
				Events.RoundEndEvent += EventHandlers.OnRoundEnd;
				Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
				Info($"Sample plugin loaded. c:");
			}
			catch (Exception e)
			{
				//This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
				Error($"There was an error loading the plugin: {e}");
			}
		}

		public override void OnDisable()
		{
			Events.RoundStartEvent -= EventHandlers.OnRoundStart;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;
			
			EventHandlers = null;
		}

		public override void OnReload()
		{
			//This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
		}

		public override string getName { get; } = "Sample Plugin";
	}
}