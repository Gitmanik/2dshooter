using UnityEngine;

namespace Gitmanik.FTPUploader
{
    public class FTPUploaderData : ScriptableObject
    {
        public string address;
        public string username, password;
        public string filename;
    }
}