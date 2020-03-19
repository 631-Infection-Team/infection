using UnityEngine;
using Mirror;
using System.Collections;

/*
	Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
	Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

namespace Infection
{
    public class NetAuthenticator : NetworkAuthenticator
    {
        public string loginUsername = "";
        public string loginPassword = "";

        #region Server

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        }

        public override void OnServerAuthenticate(NetworkConnection conn) { }

        public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
        {
            if (msg.username == loginUsername && msg.password == loginPassword)
            {
                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 100,
                    message = "Success"
                };

                conn.Send(authResponseMessage);

                OnServerAuthenticated.Invoke(conn);
            }
            else
            {
                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 200,
                    message = "Invalid Credentials"
                };

                conn.Send(authResponseMessage);
                conn.isAuthenticated = false;

                StartCoroutine(DelayedDisconnect(conn, 1));
            }
        }

        #endregion

        #region Client

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }

        public override void OnClientAuthenticate(NetworkConnection conn)
        {
            AuthRequestMessage authRequestMessage = new AuthRequestMessage
            {
                username = loginUsername,
                password = loginPassword
            };

            conn.Send(authRequestMessage);
        }

        public IEnumerator DelayedDisconnect(NetworkConnection conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            conn.Disconnect();
        }

        public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
        {
            if (msg.code == 100)
            {
                Debug.LogFormat("Authentication Response: {0}", msg.message);

                OnClientAuthenticated.Invoke(conn);
            }
            else
            {
                Debug.LogErrorFormat("Authentication Response: {0}", msg.message);

                conn.isAuthenticated = false;
                conn.Disconnect();
            }
        }

        #endregion
    }
}