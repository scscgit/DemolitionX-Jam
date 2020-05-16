using SQLite4Unity3d;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerDatabase
{
    /// <summary>
    /// Holds Player Id of online Players
    /// </summary>
    public static List<long> online;
    public static string path;
    public static bool Loaded;

    public static void StartDataBase()
    {
        online = new List<long>();
        string dir = Directory.GetCurrentDirectory() + "/.Database";
        path = dir + @"/PlayerList.db";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
        {
            if (con.GetTableInfo(nameof(PlayerData)).Count == 0)
                con.CreateTable<PlayerData>();
            Debug.Log("DataBase Started");
            Loaded = true;
        }
    }

    internal static void InsertPlayerData(PlayerData data)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            con.Insert(data);
    }
    public static bool PlayerIDExist(long playerID)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
        {
            return con.Table<PlayerData>().Where(x => x.PlayerID == playerID).Count() > 0;
        }
    }

    public static KeyValuePair<bool, PlayerData> GetPlayerData(long PlayerID)
    {
        PlayerData data = null;
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            data = con.Get<PlayerData>(PlayerID);
        return new KeyValuePair<bool, PlayerData>(data != null, data);
    }

    public static bool Authorise(long PlayerID, string Password)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            return con.Get<PlayerData>(PlayerID).DemolitionPaasword == Password;
    }

    /// <summary>
    /// Returns primary Player ID for the given id.
    /// </summary>
    /// <param name="FBID">If FBID is true given id is checked in fb IDs. If false Google id list is checked</param>
    public static long GetPlayerID(string username, bool isEmail = false)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
        {
            if (isEmail)
                return con.Table<PlayerData>().Where(x => x.DemolitionEmail == username).FirstOrDefault().PlayerID;
            else
                return con.Table<PlayerData>().Where(x => x.DemolitionID == username).FirstOrDefault().PlayerID;
        }
    }

    /// <summary>
    /// Returns bool presenting existance.
    /// </summary>
    /// <param name="FBID">If FBID is true given id is checked in fb IDs. If false Google id list is checked</param>
    public static bool UsernameExist(string id)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            return con.Table<PlayerData>().Where(x => x.DemolitionID == id).Count()>0;
    }

    public static bool PlayerEmailExist(string mail)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            return con.Table<PlayerData>().Where(x => x.DemolitionEmail == mail).Count() > 0;
    }
    public static void AddPlayer(long PlayerID, string userid, string email, string DemolitionPaasword)
    {
        var data = new PlayerData(PlayerID, userid, email, DemolitionPaasword);
        InsertPlayerData(data);
        online.Add(PlayerID);
    }

    /// <summary>
    /// Returns key pair with bool state true if player is signed and plsyer id is returned.....
    /// </summary>
    public static KeyValuePair<bool, long> SignPlayerIn(string Userid, string DemolitionPaasword)
    {
        if (UsernameExist(Userid))
        {
            var id = GetPlayerID(Userid);
            if (!Authorise(id, DemolitionPaasword))
                return new KeyValuePair<bool, long>(false, 0);
            return new KeyValuePair<bool, long>(LoadPlayerData(id), id);
        }
        if (PlayerEmailExist(Userid))
        {
            var id = GetPlayerID(Userid, true);
            if (!Authorise(id, DemolitionPaasword))
                return new KeyValuePair<bool, long>(false, 0);
            return new KeyValuePair<bool, long>(LoadPlayerData(id), id);
        }
        return new KeyValuePair<bool, long>(false, 0);
    }

    public static void SignPlayerOut(long PlayerID)
    {
        if (online.Contains(PlayerID))
            online.Remove(PlayerID);
    }

    public static void AddPlayerUsername(long PlayerID, string userid, string DemolitionEmail)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
        {
            var obj = con.Get<PlayerData>(PlayerID);
            obj.DemolitionID = userid;
            obj.DemolitionEmail = DemolitionEmail;
            UpdatePlayerData(obj);
        }
    }

    public static bool LoadPlayerData(long playerID)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            if (con.Table<PlayerData>().Where(x => x.PlayerID == playerID).Count() == 0)
                return false;
        if (online.Contains(playerID))
            return true;
        online.Add(playerID);
        return true;
    }

    public static void UpdatePlayerData(PlayerData PlayerID)
    {
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            con.Update(PlayerID);
    }

    public static void RemovePlayerData(long PlayerID)
    {
        if (online.Contains(PlayerID))
            online.Remove(PlayerID);
        using (var con = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
            con.Delete<PlayerData>(PlayerID);
    }

    public static void Dispose()
    {
        online.Clear();
        online = null;
    }
}

[Table(nameof(PlayerData))]
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
