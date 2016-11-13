using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption
{
    public class ThingDef_Readables : ThingDef
    {
        //Books
        public List<string> BookText = new List<string>();
        public List<ThingDef> BooksList = new List<ThingDef>();
        public bool IsABook = false;
        public string CloseTexture = "";
    }
}
