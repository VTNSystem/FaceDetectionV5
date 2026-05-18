using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceDetection.Models
{
    public class User
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool Active { get; set; }

        public User() { }

        public User(string username, string fullName, string nickname, bool gender, DateTime dateOfBirth, DateTime joinedDate, bool active)
        {
            Username = username;
            FullName = fullName;
            NickName = nickname;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            JoinedDate = joinedDate;
            Active = active;
        }
    }

    public class AbUser
    {
        public string Username { get; set; }
        public string FullName { get; set; }
    }
}
