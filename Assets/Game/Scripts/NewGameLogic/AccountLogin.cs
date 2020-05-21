using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class AccountLogin : MonoBehaviour
{
    public static AccountLogin Login;
    public InputField username, Password, email, newusername, newPassword, ComfirmPassword;
    public Text AccountError;

    public Text NewAccountError;

    // Use this for initialization
    void Start()
    {
        Login = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LoginAccount()
    {
        NetworkClient.Send(new AuthRequestMessage()
            {username = username.text, password = Password.text, newLogin = false});
    }

    public void NewLogin()
    {
        NetworkClient.Send(new AuthRequestMessage()
        {
            email = email.text, username = newusername.text, password = newPassword.text,
            comfirmPassword = ComfirmPassword.text, newLogin = true
        });
    }

    public void OnServerReceiveLoginMessage(NetworkConnection conn, AuthRequestMessage message)
    {
        if (message.newLogin)
            InitNewAccoutLogin(conn, message.email, message.username, message.password, message.comfirmPassword);
        else
            InitLogin(conn, message.username, message.password);
    }

    public void OnClientReceiveLoginResponseMessage(NetworkConnection conn, AuthResponseMessage message)
    {
        if (message.success)
        {
            RpcEnterLobby(message.PlayerID);
        }
        else
            ShowErrors(message.errors);
    }

    public void ShowErrors(string error)
    {
        AccountError.text = error;
        NewAccountError.text = error;
    }

    public void InitLogin(NetworkConnection conn, string username, string Password)
    {
        var pair = PlayerDatabase.SignPlayerIn(username, Password);
        string error = string.Empty;
        bool _success = true;
        int ErrorCount = 0;
        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(Password))
        {
            error = "*All fields must be filled";
            ErrorCount++;
        }

        if (!PlayerDatabase.UsernameExist(username) && !PlayerDatabase.PlayerEmailExist(username))
        {
            if (error != string.Empty)
                error += "   *Username not Found";
            else
                error += "*Username not Found";
            ErrorCount++;
        }

        if (!pair.Key)
        {
            if (error != string.Empty)
                error += "   *Incorrect Password";
            else
                error += "*Incorrect Password";
            ErrorCount++;
        }

        if (ErrorCount > 0)
            _success = false;
        conn.Send(new AuthResponseMessage() {errors = error, PlayerID = pair.Value, success = _success});
    }

    public void InitNewAccoutLogin(NetworkConnection conn, string email, string username, string Password,
        string comfirmPassword)
    {
        string error = string.Empty;
        bool _success = true;
        int ErrorCount = 0;
        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(Password) ||
            string.IsNullOrEmpty(comfirmPassword))
        {
            error = "*All fields must be filled";
            ErrorCount++;
        }

        if (Password != comfirmPassword)
        {
            if (error != string.Empty)
                error += "   *Comfirm Password Dosent Match";
            else
                error += "*Comfirm Password Dosent Match";
            ErrorCount++;
        }

        if (PlayerDatabase.UsernameExist(username))
        {
            if (error != string.Empty)
                error += "   *Username is already used";
            else
                error += "*Username is already used";
            ErrorCount++;
        }

        if (PlayerDatabase.PlayerEmailExist(email))
        {
            if (error != string.Empty)
                error += "   *Email is already used";
            else
                error += "*Email is already used";
            ErrorCount++;
        }

        if (ErrorCount > 0)
            _success = false;
        var id = CoreManager.Core.FreeIDList[0];
        PlayerDatabase.AddPlayer(id, username, email, Password);
        CoreManager.Core.FreeIDList.Remove(id);

        conn.Send(new AuthResponseMessage() {errors = error, PlayerID = id, success = _success});
    }

    public void Logout(LogoutRequestMessage msg)
    {
        PlayerDatabase.SignPlayerOut(msg.playerID);
    }

    public void RpcEnterLobby(long PlayerID)
    {
        print("new scene");
        CoreManager.Core.PlayerID = PlayerID;
        PlayerPrefs.SetString("UPlayerID", PlayerID.ToString());
        CoreManager.Core.LoadScene("Lobby");
    }
}
