using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmicApiModel
{
    public enum CosController
    {
        Plan,
        Account,
        FileSQL,
        Login,
        Platform,
        ReckonDesktop,
        Ezzy,
        Qbo,
        QBOEzzyAccount,
        ReckonEzzy
    }

    public enum Method
    {
        DoLogin,
        Save,
        Get,
        GetAll,
        ActivateAccount,
        ValidateActivationCode,
        IsExistusername,
        //ForgetPassword,
        ValidateActivationCodeInLoginMaster,
        //IsExistPassword,
        GetAccountSubscribedPlan,
        GetEzzyLogin,
        UpdateUserProfile,
        ChangePassword,
        AddEditUser,
        ProcessForgotPass

    }

    public enum Platform
    {
        ReckonDesktop,
        ReckonHosted,
        QBOAustralia,
        QBOUS
    }
}
