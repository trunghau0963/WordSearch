// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using i5.Toolkit.Core.OpenIDConnectClient;
// using i5.Toolkit.Core.ServiceCore;


// public class GoogleSignInOauth : BaseServiceBootstrapper
// {

//     [SerializeField] private ClientDataObject googleClientDataObject;
//     [SerializeField] private ClientDataObject googleClientDataObjectEditorOnly;
//     protected override void RegisterServices()
//     {

//         OpenIDConnectService oidc = new();
//         oidc.OidcProvider = new GoogleOidcProvider();
//         #if !UNITY_EDITOR
//         oidc.OidcProvider.ClientData = googleClientDataObject.ClientData;
//         #else
//         oidc.OidcProvider.ClientData = googleClientDataObjectEditorOnly.clientData;
//         oidc.RedirectURI = "https://www.google.com/";
//         #endif
//     }

//     protected override void UnRegisterServices()
//     {
//         throw new System.NotImplementedException();
//     }

//     // Start is called before the first frame update



//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
