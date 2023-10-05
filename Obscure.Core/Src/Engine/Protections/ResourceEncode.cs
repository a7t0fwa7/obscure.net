using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;

using Obscure.Core.Utility;

namespace Obscure.Core.Engine.Protections
{
    public class ResourceEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            for (int i = 0; i < Context.Module.Resources.Count; ++i)
            {
                Resource Resource = Context.Module.Resources[i];

                string Name = $"{Utils.RandomString(16)}.resources";

                Stream ResourceStream = Assembly.LoadFile(Context.Module.Location)
                    .GetManifestResourceStream(Resource.Name);

                if (ResourceStream == null)
                    return;

                using (StreamReader Reader = new StreamReader(ResourceStream))
                {
                    byte[] Data = Encoding.UTF8.GetBytes(Reader.ReadToEnd());

                    Context.Module.Resources.Add(new EmbeddedResource(
                        Name, Data));

                    Context.Module.Resources.Remove(Resource);
                }
            }
        }

        public ResourceEncode() => Name = "Resource Encryption";
    }
}
