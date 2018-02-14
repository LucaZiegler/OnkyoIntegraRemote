namespace AppOnkyo.OBJECTS
{
    public class SetupOption
    {
        public string Title;
        public int Icon;
        public string Cmd;
        public SetupOptionEntry[] LiEntries;

        public class SetupOptionEntry
        {
            public string Title;
            public string Cmd;
        }
    }
}