using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;

[System.Serializable]
public class DiscordJoinEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordSpectateEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordJoinRequestEvent : UnityEngine.Events.UnityEvent<DiscordRpc.DiscordUser> { }

[ModTitle("RaftRichPresence")]
[ModDescription("Adds discord's Rich Presence to your game, so you can show off what your upto in raft")]
[ModAuthor(". Marsh.Mello .")]
[ModIconUrl("https://www.shareicon.net/data/2017/06/21/887435_logo_512x512.png")]
[ModWallpaperUrl("https://github.com/MarshMello0/RaftRichPresence/blob/master/Banner.png?raw=true")]
[ModVersion("1.0.0")]
[RaftVersion("Update 9 (3556813)")]
public class RaftRichPresence : Mod
{
    public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
    private const string DiscordAppID = "546118094406418442";
    public DiscordRpc.DiscordUser joinRequest;
    public DiscordJoinEvent onJoin;
    public DiscordJoinEvent onSpectate;
    public DiscordJoinRequestEvent onJoinRequest;

    private DiscordRpc.EventHandlers handlers;

    private int time;
    IEnumerator Start()
    {
        SceneManager.sceneLoaded += SceneLoaded;
        handlers = new DiscordRpc.EventHandlers();
        handlers.disconnectedCallback += DisconnectedCallback;
        handlers.errorCallback += ErrorCallback;
        handlers.joinCallback += JoinCallback;
        handlers.requestCallback += RequestCallback;
        DiscordRpc.Initialize(DiscordAppID, ref handlers, true, SteamUser.GetSteamID().ToString());

        yield return new WaitForSeconds(0.5f);//Waiting half a second if the user has loaded more than one mod at once
        SetTime();
        UpdateDiscord("In Menu");
        StartCoroutine(UpdateLoop());
        
        RConsole.Log("Discord Rich Presence Mod Loaded");
    }

    public void DisconnectedCallback(int errorCode, string message)
    {
        RConsole.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
    }

    public void ErrorCallback(int errorCode, string message)
    {
        RConsole.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
    }

    public void JoinCallback(string secret)
    {
        RConsole.Log(string.Format("Discord: join ({0})", secret));
    }

    public void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        RConsole.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
        joinRequest = request;
    }

    private int ModsCount()
    {
        List<ModInfo> modList = FindObjectOfType<MainMenu>().modList;
        int modsCount = 0;
        foreach (ModInfo mod in modList)
        {
            if (mod.modState == ModInfo.ModStateEnum.running)
            {
                modsCount++;
            }
        }
        modsCount--;
        return modsCount;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "MainScene")
        {
            UpdateDiscord("In " + GameModeValueManager.GetCurrentGameModeValue().gameMode.ToString());
            SetTime();
        }
        else if (scene.name == "MainMenuScene")
        {
            UpdateDiscord("In Menu");
            SetTime();
        }
    }

    IEnumerator UpdateLoop()
    {
        yield return new WaitForSeconds(15);
        UpdateInfo();
        StartCoroutine(UpdateLoop());
    }

    private void UpdateInfo()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "MainScene")
            {
                UpdateDiscord("In " + GameModeValueManager.GetCurrentGameModeValue().gameMode.ToString());
            }
            else if (SceneManager.GetSceneAt(i).name == "MainMenuScene")
            {
                UpdateDiscord("In Menu");
            }
        }
    }

    private void SetTime()
    {
        DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        time = (int)(DateTime.UtcNow - d).TotalSeconds;
    }

    private void UpdateDiscord(string details)
    {
        Presence.details = details;
        int modCount = ModsCount();
        string state = "";
        if (modCount == 0)
        {
            state = "Using no mods :(";
        }
        else if (modCount == 1)
        {
            state = "Using " + modCount + " mod";
        }
        else
        {
            state = "Using " + modCount + " mods";
        }
        Presence.state = state;
        Presence.startTimestamp = time;
        Presence.largeImageKey = "rmllogo";
        Presence.largeImageText = "discord.gg/raft";
        Presence.smallImageKey = "characters";
        Presence.smallImageText = "www.raftmodding.com";
        //Presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM= "; Cant seem to get working
        DiscordRpc.UpdatePresence(Presence);
    }



    private void Update()
    {
        DiscordRpc.RunCallbacks();
    }

    public void OnModUnload()
    {
        DiscordRpc.Shutdown();
        StopAllCoroutines();
        RConsole.Log("RaftDiscordRichPresence has been unloaded!");
        Destroy(gameObject);
    }
}
