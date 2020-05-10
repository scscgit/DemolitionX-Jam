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

    public void NewLogin()
    {
        if (!IsNewAccountVaild())
            return;
        CoreManager.Core.CmdInitNewAccoutLogin(email.text, newusername.text, newPassword.text);
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
}
