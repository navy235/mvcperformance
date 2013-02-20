using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MinCssAndJs.Models
{
    public class Member
    {

        public string UserName { get; set; }

        [UIHint("Password")]
        public string Password { get; set; }


    }
}