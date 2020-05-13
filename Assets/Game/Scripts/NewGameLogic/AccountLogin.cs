using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Facebook;

public class AccountLogin : MonoBehaviour
{
    public static AccountLogin Login;
    public InputField username, Password, email, newusername, newPassword, ComfirmPassword;
    public GameObject nullusernameerror;
    public GameObject nullPasswordError;
    public GameObject nullemailError;
    public GameObject nullnewusernameError;
    public GameObject nullnewPasswordError;
    public GameObject nullComfirmPasswordError;
    public GameObject passwordMisMatchError;
    public GameObject emailExistError;
    public GameObject usernameExistError;
    public GameObject incorrectPasswordError;
    public GameObject usernameNotExistError;
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
        if (!IsAccountVaild())
            return;
        CoreManager.Core.CallLoginCmd(new Mirror.AuthenticatorMessage() { username = username.text, password = Password.text, newLogin = false });
    }

    public void NewLogin()
    {
        if (!IsNewAccountVaild())
            return;
        CoreManager.Core.CallLoginCmd(new Mirror.AuthenticatorMessage(){ email = email.text, username= newusername.text, password = newPassword.text, newLogin = true});
    }

    public bool IsAccountVaild()
    {
        int ErrorCount = 0;
        if (string.IsNullOrEmpty(username.text))
        {
            nullnewusernameError.SetActive(true);
            ErrorCount++;
        }
        if (string.IsNullOrEmpty(Password.text))
        {
            nullnewPasswordError.SetActive(true);
            ErrorCount++;
        }
        if (ErrorCount > 0)
            return false;
        return true;
    }

    public bool IsNewAccountVaild()
    {
        int ErrorCount = 0;
        if (string.IsNullOrEmpty(email.text))
        {
            nullemailError.SetActive(true);
            ErrorCount++;
        }
        if (string.IsNullOrEmpty(newusername.text))
        {
            nullnewusernameError.SetActive(true);
            ErrorCount++;
        }
        if (string.IsNullOrEmpty(newPassword.text))
        {
            nullnewPasswordError.SetActive(true);
            ErrorCount++;
        }
        if (string.IsNullOrEmpty(ComfirmPassword.text))
        {
            nullComfirmPasswordError.SetActive(true);
            ErrorCount++;
        }
        if (newPassword.text != ComfirmPassword.text)
        {
            passwordMisMatchError.SetActive(true);
            ErrorCount++;
        }
        if (ErrorCount > 0)
            return false;
        return true;
    }

    public void ErrorEmailExist()
    {
        emailExistError.SetActive(true);
    }

    public void ErrorUsernameExist()
    {
        usernameExistError.SetActive(true);
    }

    public void ErrorUsernameNotExist()
    {
        usernameNotExistError.SetActive(true);
    }

    public void ErrorIncorrectPassword()
    {
        incorrectPasswordError.SetActive(true);
    }
}
