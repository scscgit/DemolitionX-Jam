using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Mirror;
using SQLite4Unity3d;

public class PlayerDatabase : IDisposable
{
    private static SQLiteConnection con;
    private static List<long> PlayerIds = new List<long>();
    private static Dictionary<string,string> Usernames = new Dictionary<string, string>();
    private static Dictionary<long, PlayerData> playerDatabase = new Dictionary<long, PlayerData>();

    public static void StartDataBase()
    {
        string dir = Directory.GetCurrentDirectory() + "/.Database";
        string path = string.Format(@"URI=file:{0}/PlayerList.db", dir);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        if(con.GetTableInfo("PlayersData").Count == 0)
            con.CreateTable<PlayerData>();
        PlayerIds = con.Table<PlayerData>().Select(x => x.PlayerID).ToList();
        Usernames = con.Table<PlayerData>().Where(x => !string.IsNullOrEmpty(x.DemolitionID)).ToDictionary(x =>x.DemolitionID,x=> x.DemolitionEmail);
    }

    internal static void InsertPlayerData(PlayerData data)
    {
        con.Insert(data);
    }

    public static bool PlayerExist(long PlayerID)
    {
        return PlayerIds.Contains(PlayerID);
    }

    /// <summary>
    /// Returns key pair with bool state true if player is online and the player data.....
    /// </summary>
    public static KeyValuePair<bool, PlayerData> GetPlayerData(long PlayerID)
    {
        PlayerData tmp;
        var active = playerDatabase.ContainsKey(PlayerID);
        if (active)
        {
            tmp = playerDatabase[PlayerID];
        }
        else
            tmp = con.Get<PlayerData>(PlayerID);
        return new KeyValuePair<bool, PlayerData>(active,tmp);
    }

    public static bool Authorise(long PlayerID, string Password)
    {
        return con.Get<PlayerData>(PlayerID).DemolitionPaasword == Password;
    }

    /// <summary>
    /// Returns primary Player ID for the given id.
    /// </summary>
    /// <param name="FBID">If FBID is true given id is checked in fb IDs. If false Google id list is checked</param>
    public static long GetPlayerID(string id)
    {
        return con.Table<PlayerData>().Where(x => x.DemolitionID == id).FirstOrDefault().PlayerID;
    }

    /// <summary>
    /// Returns bool presenting existance.
    /// </summary>
    /// <param name="FBID">If FBID is true given id is checked in fb IDs. If false Google id list is checked</param>
    public static bool PlayerExist(string id)
    {
        return Usernames.ContainsKey(id);
    }

    public static bool PlayerEmailExist(string mail)
    {
        return Usernames.ContainsValue(mail);
    }
    public static void AddPlayer(long PlayerID, string userid, string email,string DemolitionPaasword)
    {
        PlayerIds.Add(PlayerID);
        Usernames[userid] = email;
        var data = new PlayerData(PlayerID, userid, email, DemolitionPaasword);
        InsertPlayerData(data);
        playerDatabase[PlayerID] = data;
    }

    /// <summary>
    /// Returns key pair with bool state true if player is signed and a message if not.....
    /// </summary>
    public static KeyValuePair<bool, string> SignPlayerIn(string Userid, string Email, string DemolitionPaasword)
    {
        if (Usernames.ContainsKey(Userid))
        {
            var id = GetPlayerID(Userid);
            if(!Authorise(id, DemolitionPaasword))
                return new KeyValuePair<bool, string>(false, "Failed Invalid Password");
            return new KeyValuePair<bool, string>(LoadPlayerData(id),"Success");
        }
        if (Usernames.ContainsValue(Email))
        {
            var id = GetPlayerID(Usernames.Where(x => x.Value == Email).FirstOrDefault().Key);
            if (!Authorise(id, DemolitionPaasword))
                return new KeyValuePair<bool, string>(false, "Failed Invalid Password");
            return new KeyValuePair<bool, string>(LoadPlayerData(id), "Success");
        }
        return new KeyValuePair<bool, string>(false, "Failed Invalid Username or Password");
    }

    public static void SignPlayerOut(long PlayerID)
    {
        if (playerDatabase.ContainsKey(PlayerID))
             playerDatabase.Remove(PlayerID);
    }

    public static void AddPlayerUsername(long PlayerID, string userid, string DemolitionEmail)
    {
        Usernames[userid] = DemolitionEmail;
        playerDatabase[PlayerID].DemolitionEmail = DemolitionEmail;
    }

    public static bool LoadPlayerData(long PlayerID)
    {
        if (!PlayerExist(PlayerID))
            return false;
        if (playerDatabase.ContainsKey(PlayerID))
            return true;
        var dat = con.Get<PlayerData>(PlayerID);
        PlayerIds.Add(PlayerID);
        playerDatabase[PlayerID] = dat;
        return true;
    }

    public static void UpdatePlayerData(PlayerData PlayerID)
    {
        con.Update(PlayerID);
    }

    public static void RemovePlayerData(long PlayerID)
    {
        if (PlayerIds.Contains(PlayerID))
            PlayerIds.Remove(PlayerID);
        if (playerDatabase.ContainsKey(PlayerID))
            playerDatabase.Remove(PlayerID);
        con.Delete<PlayerData>(PlayerID);
    }

    public void Dispose()
    {
        foreach (var data in playerDatabase)
            UpdatePlayerData(data.Value);
        playerDatabase.Clear();
        playerDatabase = null;
        con.Close();
        con.Dispose();
        con = null;
    }
}

[Serializable]
public class PlayerData
{
    [PrimaryKey]
    public long PlayerID { get; set; }
    public string DemolitionID { get; set; }
    public string DemolitionPaasword { get; set; }
    public string DemolitionEmail { get; set; }
    public string playerName { get; set; }
    public int playerAge { get; set; }
    public int playerGender { get; set; }
    public int playerMoney { get; set; }
    public int playerScore { get; set; }
    public int playedTime { get; set; }
    public int noWrecks { get; set; }
    public int noPlayersWrecked { get; set; }
    public int noWins { get; set; }
    public int matchesPlayed { get; set; }
    public string purchases { get; set; }

    public PlayerData()
    {
    }

    public PlayerData(long PlayerID)
    {
        this.PlayerID = PlayerID;
    }
    public PlayerData(long PlayerID, string DemolitionID, string DemolitionEmail, string DemolitionPaasword)
    {
        this.PlayerID = PlayerID;
        this.DemolitionID = DemolitionID;
        this.DemolitionEmail = DemolitionEmail;
        this.DemolitionPaasword = DemolitionPaasword;
    }

    public override string ToString()
    {
        return string.Format("[Person: PlayerID={0},DemolitionID={1},DemolitionPaasword={2},Email={3},playerName={4},playerAge={5},playerGender={6},playerMoney={7},playerScore={8},playedTime ={9}, noWrecks ={10},noPlayersWrecked ={11}, noWins ={12}, matchesPlayed ={13}]",
            PlayerID, DemolitionID, DemolitionPaasword, DemolitionEmail, playerName, playerAge, playerGender, playerMoney, playerScore, playedTime, noWrecks, noPlayersWrecked, noWins, matchesPlayed, purchases);
    }
}
