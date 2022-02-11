using NintendoSpy.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintendoSpy
{
    public class InputSource
    {
        static public readonly InputSource GAMECUBE = new InputSource ("gamecube", "GameCube", true, false, port => new SerialControllerReader (port, GameCube.ReadFromPacket));

        static public readonly IReadOnlyList <InputSource> ALL = new List <InputSource> {
            GAMECUBE
        };

        static public readonly InputSource DEFAULT = GAMECUBE;

        public string TypeTag { get; private set; }
        public string Name { get; private set; }
        public bool RequiresComPort { get; private set; }
        public bool RequiresId { get; private set; }

        public Func <string, IControllerReader> BuildReader { get; private set; }

        InputSource (string typeTag, string name, bool requiresComPort, bool requiresId, Func <string, IControllerReader> buildReader) {
            TypeTag = typeTag;
            Name = name;
            RequiresComPort = requiresComPort;
            RequiresId = requiresId;
            BuildReader = buildReader;
        }
    }
}
