using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public enum ShoulderPadType
    {
        Both,
        Right,
        Left
    }

    public class CompProperties_PauldronDrawer : CompProperties
    {
        public ShoulderPadType shoulderPadType;
        public ShaderType shaderType;
        public string PadTexPath;       
        
            
    }
}
