using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WolvenKit.CR2W.Types;

namespace Cr2wTools_RTTIExporter
{
    public partial class Form1 : Form
    {
        List<CR2WCLassFormat> classes = new List<CR2WCLassFormat>();
        List<string> b = new List<string>();
        List<string> bc = new List<string>();
        List<string> s = new List<string>();
        List<string> sc = new List<string>();
        List<string> excludedclasses = new List<string>();



        string outdir;

        public Form1()
        {
            InitializeComponent();

            // Buffered Types
            b.Add("CAnimPointCloudLookAtParam");
            b.Add("CAreaComponent");
            b.Add("CBehaviorGraph");
            b.Add("CBehaviorGraphBlendMultipleNode");
            b.Add("CBehaviorGraphContainerNode");
            b.Add("CBehaviorGraphStateMachineNode");
            b.Add("CCameraCompressedPose");
            b.Add("CClipMap");
            b.Add("CComponent");
            b.Add("CCookedExplorations");
            b.Add("CCurve");
            b.Add("CCutsceneTemplate");
            b.Add("CEntityTemplate");
            b.Add("CEvaluatorFloatCurve");
            b.Add("CExtAnimEventsFile");
            b.Add("CFoliageResource");
            b.Add("CGameWorld");
            b.Add("CLayerGroup");
            b.Add("CMaterialGraph");
            b.Add("CMaterialInstance");
            b.Add("CMesh");
            b.Add("CNode");
            b.Add("CParticleEmitter");
            b.Add("CPhysicsDestructionResource");
            b.Add("CRagdoll");
            b.Add("CSkeletalAnimationSetEntry");
            b.Add("CStorySceneSection");
            b.Add("CSkeletalAnimation");
            b.Add("CSkeletalAnimationSet");
            b.Add("CSwfResource");
            b.Add("CUmbraScene");
            b.Add("CUmbraTile");
            b.Add("CWayPointsCollectionsSet");

            bc.Add("CBitmapTexture");
            bc.Add("CCubeTexture");
            bc.Add("CEntity");
            bc.Add("CFont");
            bc.Add("CFXTrackItem");
            bc.Add("CGenericGrassMask");
            bc.Add("CLayerInfo");
            bc.Add("CPhysicalCollision");
            bc.Add("CSectorData");
            bc.Add("CSkeleton");
            bc.Add("CStorySceneScript");
            bc.Add("CSwfTexture");
            bc.Add("CTerrainTile");
            bc.Add("CTextureArray");
            bc.Add("CWayPointsCollection");
            bc.Add("SAppearanceAttachment");
            bc.Add("CSwarmCellMap");



            // structs
            //s.Add("SDynamicDecalMaterialInfo");
            
            //s.Add("SBoneIndiceMapping");
            //s.Add("EmitterDurationSettings");
            //s.Add("EmitterDelaySettings");
            //s.Add("CParticleSystem");
            //s.Add("CDecalSpawner");
            s.Add("SFoliageInstance");

            excludedclasses.Add("TagList");
            excludedclasses.Add("EngineTransform");
            excludedclasses.Add("EngineQsTransform");

            // Excluded
            excludedclasses.Add("CVariant");
            excludedclasses.Add("CColorShift");
            excludedclasses.Add("Color");
            excludedclasses.Add("CIndexed2dArray"); //???
            //excludedclasses.Add("Vector3"); //???
            excludedclasses.Add("Matrix"); //???
            excludedclasses.Add("SCurveData");
            excludedclasses.Add("CEnvProbeComponent");


            //excludedclasses.Add("SAnimationBufferBitwiseCompressedBoneTrack"); //???


        }

        private void toolStripMenuItemRun_Click(object sender, EventArgs e)
        {
            //get output directory
            var dlg = new FolderBrowserDialog
            {
               
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                outdir = dlg.SelectedPath;

                if (!Directory.Exists(outdir))
                    return;

                //get xml to read
                string path = OpenXML();

                if (!String.IsNullOrEmpty(path) && File.Exists(path))
                {
                    classes = ReadXML(path);
                    textBoxOutput.AppendText("Finished exporting rtti classes.\r\n");
                }
                else
                    textBoxOutput.AppendText("xml does not exist.\r\n");
            }
        }

        private string OpenXML()
        {
            string file = "";


            var dlg = new OpenFileDialog
            {
                Title = "Select rtti_dump.xml.",
                FileName = "",
                Filter = "*.xml|*.xml"
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                file = dlg.FileName;
            }

            return file;
        }

        private List<CR2WCLassFormat> ReadXML(string file)
        {
            using (StreamReader sr = new StreamReader(file, true))
            {
                List<CR2WCLassFormat> classes = new List<CR2WCLassFormat>();

                XDocument xdoc = XDocument.Load(sr);

                List<XElement> xmlclasses = xdoc.Descendants("class").ToList(); //all classes
                int classCount = xmlclasses.Count;

                //progress bar
                toolStripProgressBar.Maximum = classCount * 2;
                toolStripProgressBar.Minimum = 0;
                toolStripProgressBar.Step = 1;
                
                for (int i = 0; i < classCount; i++)
                {

                    CR2WCLassFormat data = new CR2WCLassFormat();

                    //add xml class attributes to class data
                    List<XAttribute> attributes = xmlclasses[i].Attributes().ToList();
                    data.Name = attributes.Find(x => x.Name.LocalName.Equals("Name"))?.Value;

                   

                    data.ParentName = attributes.Find(x => x.Name.LocalName.Equals("BaseClass"))?.Value;

                    

                    //add xml properties to class data
                    List<CR2WPropertyFormat> properties = new List<CR2WPropertyFormat>();
                    List<XElement> elements = xmlclasses[i].Elements()
                        .Where(x => x.Name.LocalName.Equals("property"))
                        .ToList();
                    foreach (XElement el in elements)
                    {
                        

                        CR2WPropertyFormat propdata = new CR2WPropertyFormat();

                        List<XAttribute> propattributes = el.Attributes().ToList();
                        propdata.Name = propattributes.Find(x => x.Name.LocalName.Equals("Name"))?.Value;

                        

                        propdata.Type = propattributes.Find(x => x.Name.LocalName.Equals("Type"))?.Value;
                        propdata.IsEditable = bool.Parse(propattributes.Find(x => x.Name.LocalName.Equals("IsEditable"))?.Value);
                        propdata.IsScripted = bool.Parse(propattributes.Find(x => x.Name.LocalName.Equals("IsScripted"))?.Value);


                        // skip scripted and not editable properties
                        // WHY??
                        //if (propdata.IsScripted && !propdata.IsEditable)
                        //    continue;

                        //check invalid var names
                        propdata.NormalizedName = NormalizeName(propdata.Name);

                        properties.Add(propdata);
                    }
                    data.AllProperties = properties;

                    //add xml data to all classes list
                    classes.Add(data);

                    //log
                    //textBoxOutput.AppendText($"exported {i + 1}/{classCount} classes from RTTI.\r\n");
                    toolStripProgressBar.PerformStep();

                }

                for (int i = 0; i < classes.Count; i++)
                {
                    CR2WCLassFormat c = classes[i];
                    c.ParentClass = classes.FirstOrDefault(_ => _.Name == c.ParentName);

                    foreach (var p in c.AllProperties)
                    {
                        if (c.ParentClass != null && c.ParentClass.AllProperties != null)
                        {
                            if (!c.ParentClass.AllProperties.Select(_ => _.Name).Contains(p.Name))
                            {
                                c.xProperties.Add(p);
                            }
                        }
                        else
                            c.xProperties.Add(p);
                    }

                    // exclude some classes
                    // always exclude those
                    if (excludedclasses.Contains(c.Name))
                    {

                    }
                    else
                    {
                        // if checked: print only partial classes
                        if (checkBox1.Checked)
                        {
                            if ((b.Contains(c.Name)
                            || bc.Contains(c.Name)
                            || s.Contains(c.Name)
                            || sc.Contains(c.Name)))
                                PrintClass(c);
                        }
                        // print only full classes
                        else
                        {
                            if (!(b.Contains(c.Name)
                            || bc.Contains(c.Name)
                            || s.Contains(c.Name)
                            || sc.Contains(c.Name)))
                                PrintClass(c);
                        }
                    }
                    

                    //log
                    //textBoxOutput.AppendText($"exported {i + 1}/{classCount} classes from RTTI.\r\n");
                    toolStripProgressBar.PerformStep();
                }

                return classes;





                string NormalizeName(string name)
                {
                    var nname = name.Replace('.', '_')
                        .Replace(' ', '_')
                        .Replace('/', '_')
                        .Replace('\'', '_')
                        .Replace('-', '_')
                        .Replace('?', '_')
                        .Replace('(', '_')
                        .Replace(')', '_')
                        .Replace('[', '_')
                        .Replace(']', '_');
                    if (Regex.IsMatch(nname, @"^\d+"))
                        nname = $"_{nname}";
                    return nname;
                }

            }
        }

        private void PrintClass(CR2WCLassFormat data)
        {
            bool isbuffered = b.Contains(data.Name);
            bool isbufferedc = bc.Contains(data.Name);
            bool isstruct = s.Contains(data.Name);
            bool isstructc = sc.Contains(data.Name);

            string finalstring = "";

            //assemble string
            string header =
                "using System.IO;\r\n" +
                "using System.Runtime.Serialization;\r\n" +
                "using WolvenKit.CR2W.Reflection;\r\n" +
                "using static WolvenKit.CR2W.Types.Enums;\r\n" +
                
                "\r\n\r\n";
            string parent = data.ParentName;
            if (!String.IsNullOrEmpty(parent))
            {
                parent = ": " + parent;
            }
            else
            {
                parent = ": " + "CVariable";
            }
            string classcons =
            $"namespace WolvenKit.CR2W.Types\r\n" +
            $"{{\r\n" +
            $"\t[DataContract(Namespace = \"\")]\r\n";

            if (isstruct || isstructc)
                classcons += $"\t[REDMeta(EREDMetaInfo.REDStruct)]\r\n";
            else
                classcons += $"\t[REDMeta]\r\n";

            if (isbuffered || isbufferedc)
                classcons += $"\tpublic partial class {data.Name} {parent}\r\n";
            else
                classcons += $"\tpublic class {data.Name} {parent}\r\n";

            classcons += $"\t{{" +
            $"\r\n";

            string properties = "";
            foreach (CR2WPropertyFormat propdata in data.xProperties)
            {
                string typ = propdata.Type;
               
                //Attributes 
                properties += "\t\t" + BuildAttributeFromRTTIRecursive(propdata);
                
                //Var declaration
                string vartype = BuildTypeFromRTTIRecursive(typ);
                string varname = propdata.NormalizedName.First().ToString().ToUpper() + propdata.NormalizedName.Substring(1); //capitalize all var names in cr2w tool
                properties += $"\t\tpublic {vartype} {varname} {{ get; set;}}" +
                                $"\r\n\r\n";
            }
            string ctor = "";
            if (isbufferedc || isstructc)
            {

            }
            else
                ctor += $"\t\tpublic {data.Name}(CR2WFile cr2w, CVariable parent, string name) : base(cr2w, parent, name)" + "{ }\r\n\r\n";

            string methods = $"\t\tpublic static new CVariable Create(CR2WFile cr2w, CVariable parent, string name) => new {data.Name}(cr2w, parent, name);\r\n\r\n";

            if (isbufferedc || isstructc)
            {
                
            }
            else
            {
                methods += "\t\tpublic override void Read(BinaryReader file, uint size) => base.Read(file, size);\r\n\r\n";

                methods += "\t\tpublic override void Write(BinaryWriter file) => base.Write(file);\r\n\r\n";
            }


            string end = "\t}\r\n}";
            
            finalstring = header + classcons  + properties + ctor +  methods + end;

            //print file
            string filename = $"{data.Name}.cs";
            string outfile = Path.Combine(outdir, filename);
            try
            {
                File.WriteAllText(outfile, finalstring);
            }
            catch
            {

            }

        }

        private string BuildAttributeFromRTTIRecursive(CR2WPropertyFormat propdata)
        {
            string name = propdata.Name; //property name
            string type = propdata.Type; //property type ( Type="Array:12,0,string" / Type="Array:12,0,Array:12,0,string" )

            //[1]Bezier2dHandle
            var regFixedSizeArray = new Regex(@"^\[(\d+)\](.+)$");
            var matchFixedSizeArray = regFixedSizeArray.Match(propdata.Type);
            if (matchFixedSizeArray.Success)
            {
                var flags = matchFixedSizeArray.Groups[1].Value;
                return $"[RED(\"{name}\", {flags})] ";
            }

            //string specType = type.Split(',').First()?.Split(':').First(); //e.g. Array
            var reg = new Regex(@"^(\w+):(.+)$");
            var match = reg.Match(type);
            if (match.Success)
            {

                switch (match.Groups[1].Value)
                {
                    case "Array":
                    case "array":
                        {
                            var resnumbers = "";
                            var regArrayType = new Regex(@"(\d+),(\d+),(.+)");
                            var matchArrayType = regArrayType.Match(type);
                            if (matchArrayType.Success)
                            {
                                resnumbers = $"{matchArrayType.Groups[1].Value},{matchArrayType.Groups[2].Value}";
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }

                            //var splitparts = type.Split(',').ToList();
                            //var resnumbers = "";
                            //foreach (string part in splitparts)
                            //{
                            //    resnumbers += new String(part.Where(Char.IsDigit).ToArray()) + ",";
                            //}
                            //resnumbers = resnumbers.TrimEnd(',');


                            return $"[RED(\"{name}\", {resnumbers})] ";
                        }
                    case "Ptr":
                    case "ptr":
                    case "Soft":
                    case "soft":
                    case "Static":
                    case "static":
                    case "Handle":
                    case "handle":
                    default:
                        {
                            return $"[RED(\"{propdata.Name}\")] ";
                        }
                }
            }
            else
                return $"[RED(\"{propdata.Name}\")] ";
        }

        private string BuildTypeFromRTTIRecursive(string rawtype)
        {


            string specType = rawtype.Split(',').First()?.Split(':').First();

            //[1]Bezier2dHandle
            var regFixedSizeArray = new Regex(@"^\[(\d+)\](.+)$");
            var matchFixedSizeArray = regFixedSizeArray.Match(rawtype);
            if (matchFixedSizeArray.Success)
            {
                return $"CArrayFixedSize<{BuildTypeFromRTTIRecursive(matchFixedSizeArray.Groups[2].Value)}>";
            }

            


            //Attributes 
            switch (specType)
            {
                case "Array":
                case "array":
                    {
                        // Type="array:12,0,array:12,0,String"
                        string type = "";
                        int count = rawtype.IndexOf(',');
                        type = rawtype.Remove(0, count + 1);
                        count = type.IndexOf(',');
                        type = type.Remove(0, count + 1); //e.g. String or array:12,0,String

                        if (type == "Uint8" || type == "Int8" || type == "sbyte" || type == "byte")
                            return $"CByteArray";

                        if (typeof(Enums).GetNestedTypes().Select(_ => _.Name).Contains(type))
                        {
                            return $"CArray<CEnum<{type}>>";
                        }

                        type = BuildTypeFromRTTIRecursive(type);

                        return $"CArray<{type}>";
                    }
                case "Static":
                case "static":
                    {
                        // Type="static:4,Uint32" 
                        string type = rawtype.Split(',').Last();

                        return $"{specType}<{type}>";
                    }
                case "Ptr":
                case "ptr":
                    {
                        string type = "";
                        type = rawtype.Split(':').Last();

                        return $"CPtr<{type}>";
                    }
                case "Soft":
                case "soft":
                    {
                        string type = "";
                        type = rawtype.Split(':').Last();

                        return $"CSoft<{type}>";
                    }
                case "Handle":
                case "handle":
                    {
                        string type = "";
                        type = rawtype.Split(':').Last();

                        return $"CHandle<{type}>";
                    }
                case "sbyte": return "CInt8";
                case "byte": return "CUInt8";
                case "Uint8": return "CUInt8";
                case "Int8": return "CInt8"; 
                case "UInt16": return "CUInt16"; 
                case "Uint16": return "CUInt16"; 
                case "Int16": return "CInt16"; 
                case "UInt32": return "CUInt32"; 
                case "Uint32": return "CUInt32"; 
                case "Int32": return "CInt32"; 
                case "UInt64": return "CUInt64"; 
                case "Uint64": return "CUInt64"; 
                case "Int64": return "CInt64"; 
                case "nool": return "CBool"; 
                case "Bool": return "CBool"; 
                case "bool": return "CBool"; 
                case "float": return "CFloat"; 
                case "Float": return "CFloat"; 
                case "string": return "CString"; 
                case "String": return "CString"; 
                case "Color": return "CColor"; 
                case "Matrix": return "CMatrix"; 

                default:
                    {
                        if (typeof(Enums).GetNestedTypes().Select(_ => _.Name).Contains(specType))
                        {
                            return $"CEnum<{specType}>";
                        }
                        else
                            return specType;
                    }
            }            
        }

        class CR2WCLassFormat
        {
            public string Name { get; set; }
            
            public string ParentName { get; set; }
            public CR2WCLassFormat ParentClass { get; set; }
            public List<CR2WPropertyFormat> xProperties { get; set; } = new List<CR2WPropertyFormat>();
            public List<CR2WPropertyFormat> AllProperties { get; set; } = new List<CR2WPropertyFormat>();

            public override string ToString()
            {
                return $"{Name},{xProperties.Count}";
            }
        }

        struct CR2WPropertyFormat
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsEditable { get; set; }
            public bool IsScripted { get; set; }
            public string NormalizedName { get; set; }

            public override string ToString()
            {
                return $"{Type}:{Name}";
            }
        }
    }

}