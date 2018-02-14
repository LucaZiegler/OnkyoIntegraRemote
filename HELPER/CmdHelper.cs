using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Util;
using AppOnkyo.ISCP;
using Base64 = Java.Util.Base64;
using Encoding = System.Text.Encoding;

namespace AppOnkyo.HELPER
{
    public class CmdHelper
    {
        public class NetOperation
        {
            public const string Com = "NTC";

            public const string PLAY = "PLAY";
            public const string STOP = "STOP";
            public const string PAUSE = "PAUSE";
            public const string TRACK_UP = "TRUP";
            public const string TRACK_DOWN = "TRDN";
            public const string FAST_FORWARD = "FF";
            public const string REWIND = "REW";
            public const string REPEAT = "REPEAT";
            public const string RANDOM = "RANDOM";
            public const string DISPLAY = "DISPLAY";

            public static string Set(string par)
            {
                return Com + par;
            }
        }

        public class NetListInfo
        {
            public const string Com = "NLS";

            public Parameter InfType;
            public Parameter Property;

            public NetListInfo(string raw)
            {
                switch (raw[0])
                {
                    case 'A':
                        InfType = Parameter.INF_TYPE_ASCII_LETTER;
                        break;
                    case 'C':
                        InfType = Parameter.INF_TYPE_CURSOR;
                        break;
                    case 'U':
                        InfType = Parameter.INF_TYPE_UNICODE_LETTER;
                        break;
                }
                switch (raw[2])
                {
                    case '-':
                        InfType = Parameter.PROP_NONE;
                        break;
                    case '0':
                        InfType = Parameter.PROP_PLAYING;
                        break;
                    case 'A':
                        InfType = Parameter.PROP_ARTIST;
                        break;
                    case 'B':
                        InfType = Parameter.PROP_ALBUM;
                        break;
                    case 'F':
                        InfType = Parameter.PROP_FOLDER;
                        break;
                    case 'M':
                        InfType = Parameter.PROP_MUSIC;
                        break;
                    case 'P':
                        InfType = Parameter.PROP_PLAYLIST;
                        break;
                    case 'S':
                        InfType = Parameter.PROP_SEARCH;
                        break;
                }
            }

            public enum Parameter
            {
                INF_TYPE_ASCII_LETTER,
                INF_TYPE_CURSOR,
                INF_TYPE_UNICODE_LETTER,
                PROP_NONE,
                PROP_PLAYING,
                PROP_ARTIST,
                PROP_ALBUM,
                PROP_FOLDER,
                PROP_MUSIC,
                PROP_PLAYLIST,
                PROP_SEARCH
            }
        }

        public class NetPopup
        {
            public const string Com = "NPU";

            public State DisplayType;
            public string Title, Message;

            public NetPopup(string raw)
            {
                switch (raw[0])
                {
                    case 'T':
                        DisplayType = State.DISPLAY_TYPE_TOP;
                        break;
                    case 'B':
                        DisplayType = State.DISPLAY_TYPE_BOTTOM;
                        break;
                    case 'L':
                        DisplayType = State.DISPLAY_TYPE_LIST;
                        break;
                }
            }

            public enum State
            {
                DISPLAY_TYPE_TOP,
                DISPLAY_TYPE_BOTTOM,
                DISPLAY_TYPE_LIST,
            }
        }

        public class NetArt
        {
            public delegate void OnArtStatusListener(int status, Bitmap bm);

            public OnArtStatusListener OnArtStatusChanged;

            public const string Com = "NJA";
            public bool isArtAvailable = false;
            public Bitmap bmArt = null;

            private string buffer = "";

            public void Clear()
            {
                isArtAvailable = false;
                bmArt = null;
                buffer = "";
                OnArtStatusChanged?.Invoke(Constants.STAT_ART_ERR, null);
            }


            public void OnArtMsgReceived(string raw)
            {
                try
                {
                    switch (raw[0])
                    {
                        case '2':
                            string url = raw.Substring(2);
                            using (WebClient client = new WebClient())
                            {
                                var bts = client.DownloadData(url);
                                var ext = bts.Skip(49).ToArray();
                                
                                bmArt = BitmapFactory.DecodeByteArray(ext, 0, ext.Length);

                                OnArtStatusChanged?.Invoke(Constants.STAT_ART_DONE, bmArt);
                            }

                            break;
                        default:

                            switch (raw[1])
                            {
                                case '0':
                                    buffer = raw.Substring(2);
                                    bmArt = null;
                                    isArtAvailable = false;
                                    OnArtStatusChanged?.Invoke(Constants.STAT_ART_LOADING, null);
                                    break;
                                case '1':
                                    buffer += raw.Substring(2);
                                    bmArt = null;
                                    break;
                                case '2':
                                    buffer += raw.Substring(2);
                                    var bts = ISCPHelper.HexStringToBytes(buffer);
                                    bmArt = BitmapFactory.DecodeByteArray(bts, 0, bts.Length);

                                    if (bmArt == null)
                                        throw new NullReferenceException();
                                    isArtAvailable = true;
                                    OnArtStatusChanged?.Invoke(Constants.STAT_ART_DONE, bmArt);
                                    buffer = "";
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    buffer = "";
                    Console.WriteLine(e);
                    OnArtStatusChanged?.Invoke(Constants.STAT_ART_ERR, null);
                }
            }
        }

        public class NetTitleName
        {
            public const string Com = "NTI";
            public static string Request => Com + "QSTN";

            public string TitleName;

            public NetTitleName(string raw)
            {
                TitleName = raw;
            }
        }

        public class NetArtistName
        {
            public const string Com = "NAT";
            public static string Request => Com + "QSTN";

            public string ArtistName;

            public NetArtistName(string raw)
            {
                ArtistName = raw;
            }
        }

        public class NetAlbumName
        {
            public const string Com = "NAL";
            public static string Request => Com + "QSTN";

            public string AlbumName;

            public NetAlbumName(string raw)
            {
                AlbumName = raw;
            }
        }

        public class NetStatus
        {
            public const string Com = "NST";
            public static string Request => Com + "QSTN";

            public Status StatusPlay;
            public Status StatusRepeat;
            public Status StatusShuffle;

            public NetStatus(string raw)
            {
                switch (raw[0])
                {
                    default:
                        StatusPlay = Status.STOP;
                        break;
                    case 'P':
                        StatusPlay = Status.PLAY;
                        break;
                    case 'p':
                        StatusPlay = Status.PAUSE;
                        break;
                }
                switch (raw[1])
                {
                    default:
                        StatusRepeat = Status.REPEAT_OFF;
                        break;
                    case 'R':
                        StatusRepeat = Status.REPEAT_ALL;
                        break;
                    case 'F':
                        StatusRepeat = Status.REPEAT_FOLDER;
                        break;
                    case '1':
                        StatusRepeat = Status.REPEAT_ONE;
                        break;
                }
                switch (raw[2])
                {
                    default:
                        StatusShuffle = Status.SHUFFLE_OFF;
                        break;
                    case 'S':
                        StatusShuffle = Status.SHUFFLE_ALL;
                        break;
                    case 'A':
                        StatusShuffle = Status.SHUFFLE_ALBUM;
                        break;
                    case 'F':
                        StatusShuffle = Status.SHUFFLE_FOLDER;
                        break;
                }
            }

            public enum Status
            {
                STOP,
                PLAY,
                PAUSE,
                REPEAT_OFF,
                REPEAT_ALL,
                REPEAT_FOLDER,
                REPEAT_ONE,
                SHUFFLE_OFF,
                SHUFFLE_ALL,
                SHUFFLE_ALBUM,
                SHUFFLE_FOLDER
            }
        }

        public class NetTime
        {
            public const string Com = "NTM";
            public TimeSpan? tsProg, tsAll;

            public NetTime(string raw)
            {
                var times = raw.Split('/');
                try
                {
                    TimeSpan t1;
                    var s1 = TimeSpan.TryParseExact(times[0], "hh':'mm':'ss", CultureInfo.InvariantCulture, out t1);
                    if (s1)
                        tsProg = t1;

                    TimeSpan t2;
                    var s2 = TimeSpan.TryParseExact(times[1], "hh':'mm':'ss", CultureInfo.InvariantCulture, out t2);
                    if (s2)
                        tsAll = t2;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class Menu
        {
            public const string Com = "OSD";

            public const string MENU = "MENU";
            public const string UP = "UP";
            public const string DOWN = "DOWN";
            public const string RIGHT = "RIGHT";
            public const string LEFT = "LEFT";
            public const string ENTER = "ENTER";
            public const string EXIT = "EXIT";
            public const string AUDIO = "AUDIO";
            public const string VIDEO = "VIDEO";
            public const string HOME = "HOME";
            public const string QUICK = "QUICK";

            public static string Set(string par)
            {
                return Com + par;
            }
        }

        public class MenuPlayer
        {
            public const string Com = "CDV";

            public const string POWER = "POWER";
            public const string POWER_ON = "PWRON";
            public const string POWER_OFF = "PWROFF";
            public const string PLAY = "PLAY";
            public const string STOP = "STOP";
            public const string SKIP_BACK = "SKIP.R";
            public const string SKIP_FORW = "SKIP.F";
            public const string FF = "FF";
            public const string REW = "REW";
            public const string PAUSE = "PAUSE";
            public const string SETUP = "SETUP";
            public const string TOPMENU = "TOPMENU";
            public const string MENU = "MENU";
            public const string UP = "UP";
            public const string DOWN = "DOWN";
            public const string RIGHT = "RIGHT";
            public const string LEFT = "LEFT";
            public const string RETURN = "RETURN";
            public const string OPEN_CLOSE = "OP/CL";
            public const string ANGLE = "ANGLE";
            public const string DISP = "DISP";
            public const string CLEAR = "DISP";

            public static string Set(string par)
            {
                return Com + par;
            }
        }

        public class MenuTV
        {
            public const string Com = "CTV";

            public const string POWER = "POWER";
            public const string POWER_ON = "PWRON";
            public const string POWER_OFF = "PWROFF";
            public const string CH_UP = "CHUP";
            public const string CH_DOWN = "CHDN";
            public const string VOL_UP = "VLUP";
            public const string VOL_DOWN = "VLDN";
            public const string MUTE = "MUTE";
            public const string DISPLAY = "DISP";
            public const string INPUT = "INPUT";
            public const string CLEAR = "CLEAR";
            public const string SETUP = "SETUP";
            public const string GUIDE_TOPMENU = "GUIDE";
            public const string PREVIOUS = "PREV";
            public const string UP = "UP";
            public const string DOWN = "DOWN";
            public const string LEFT = "LEFT";
            public const string RIGHT = "RIGHT";
            public const string ENTER = "ENTER";
            public const string RETURN = "RETURN";


            public static string Set(string par)
            {
                return Com + par;
            }
        }

        public class Power
        {
            public const string Com = "PWR";

            public static string Request => Com + "QSTN";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }

            public bool PowerState;

            public Power(string raw)
            {
                PowerState = Converter(raw);
            }
        }

        public class Volume
        {
            public const string Com = "MVL";

            public static string Request => Com + "QSTN";
            public static string Set(int pro) => Com + $"{pro:x2}";

            public static string Up => Com + "UP";
            public static string Down => Com + "DOWN";

            public static int Converter(string i)
            {
                return int.Parse(i, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public class Mute
        {
            public const string Com = "AMT";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "TG";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }

        public class Input
        {
            public string Name;
            public string Parameter;

            public const string Com = "SLI";

            public static string Request => Com + "QSTN";
            public static string Set(string par) => Com + par;
            
            public static readonly Input[] Inputs =
            {
                new Input
                {
                    Name = "BD/DVD",
                    Parameter = "10"
                },
                new Input
                {
                    Name = "CBL/SAT",
                    Parameter = "01"
                },
                new Input
                {
                    Name = "STB/DVR",
                    Parameter = "00"
                },
                new Input
                {
                    Name = "Game",
                    Parameter = "02"
                },
                new Input
                {
                    Name = "PC",
                    Parameter = "05"
                },
                new Input
                {
                    Name = "AUX1",
                    Parameter = "03"
                },
                new Input
                {
                    Name = "TV/CD",
                    Parameter = "23"
                },
                new Input
                {
                    Name = "Phono",
                    Parameter = "22"
                },
                new Input
                {
                    Name = "Network",
                    Parameter = "2B"
                },
                new Input
                {
                    Name = "AUX2",
                    Parameter = "04"
                },
                new Input
                {
                    Name = "TV/TAPE",
                    Parameter = "20"
                },
                new Input
                {
                    Name = "FM",
                    Parameter = "24"
                },
                new Input
                {
                    Name = "USB Front",
                    Parameter = "29"
                },
                new Input
                {
                    Name = "Bluetooth",
                    Parameter = "2E"
                },
                new Input
                {
                    Name = "AM",
                    Parameter = "25"
                },
                new Input
                {
                    Name = "Tuner",
                    Parameter = "26"
                },
                new Input
                {
                    Name = "Music Server",
                    Parameter = "27"
                },
                new Input
                {
                    Name = "Internet radio",
                    Parameter = "28"
                },
                new Input
                {
                    Name = "USB Rear",
                    Parameter = "2A"
                },
                new Input
                {
                    Name = "USB Toggle",
                    Parameter = "2C"
                },
                new Input
                {
                    Name = "Airplay",
                    Parameter = "2D"
                },
                new Input
                {
                    Name = "Universal Port",
                    Parameter = "40"
                },
                new Input
                {
                    Name = "Multi CH",
                    Parameter = "30"
                },
                new Input
                {
                    Name = "XM",
                    Parameter = "31"
                },
                new Input
                {
                    Name = "Sirius",
                    Parameter = "32"
                },
                new Input
                {
                    Name = "DAB",
                    Parameter = "33"
                },
                new Input
                {
                    Name = "EXTRA1",
                    Parameter = "07"
                },
                new Input
                {
                    Name = "EXTRA2",
                    Parameter = "08"
                },
                new Input
                {
                    Name = "EXTRA3",
                    Parameter = "09"
                },
            };

            public Input(string v)
            {
                Name = Converter(v);
                Parameter = v;
            }

            public Input()
            {
                
            }

            public static string Converter(string par)
            {
                return Inputs.First(it => it.Parameter == par).Name;
            }

            public static int ConverterToIndex(string par)
            {
                int ind = 0;
                foreach (var input in Inputs)
                {
                    if (input.Parameter == par)
                    {
                        return ind;
                    }
                    ind++;
                }
                return -1;
            }
        }

        public class ListeningMode
        {
            public string Name;
            public string Parameter;

            public const string Com = "LMD";

            public static string Request => Com + "QSTN";
            public static string Set(string par) => Com + par;


            public static ListeningMode[] ListeningModes =
            {
                new ListeningMode
                {
                    Name = "Stereo",
                    Parameter = "00"
                },
                new ListeningMode
                {
                    Name = "Direct",
                    Parameter = "01"
                },
                new ListeningMode
                {
                    Name = "Surround",
                    Parameter = "02"
                },
                new ListeningMode
                {
                    Name = "Film (Game-RPG)",
                    Parameter = "04"
                },
                new ListeningMode
                {
                    Name = "THX",
                    Parameter = "04"
                },
                new ListeningMode
                {
                    Name = "Action (Game-Action)",
                    Parameter = "05"
                },
                new ListeningMode
                {
                    Name = "Musical (Game-Rock)",
                    Parameter = "06"
                },
                new ListeningMode
                {
                    Name = "Mono Movie",
                    Parameter = "07"
                },
                new ListeningMode
                {
                    Name = "Orchestra",
                    Parameter = "08"
                },
                new ListeningMode
                {
                    Name = "Unplugged",
                    Parameter = "09"
                },
                new ListeningMode
                {
                    Name = "Studio-Mix",
                    Parameter = "0A"
                },
                new ListeningMode
                {
                    Name = "TV Logic",
                    Parameter = "0B"
                },
                new ListeningMode
                {
                    Name = "All Ch Stereo",
                    Parameter = "0C"
                },
                new ListeningMode
                {
                    Name = "Theater-Dimensional",
                    Parameter = "0D"
                },
                new ListeningMode
                {
                    Name = "Enhanced (Game-Sports)",
                    Parameter = "0E"
                },
                new ListeningMode
                {
                    Name = "Mono",
                    Parameter = "0F"
                },
                new ListeningMode
                {
                    Name = "Pure Audio",
                    Parameter = "11"
                },
                new ListeningMode
                {
                    Name = "Multiplex",
                    Parameter = "12"
                },
                new ListeningMode
                {
                    Name = "Full Mono",
                    Parameter = "13"
                },
                new ListeningMode
                {
                    Name = "Dolby Virtual",
                    Parameter = "14"
                },
                new ListeningMode
                {
                    Name = "DTS Surround Sensation",
                    Parameter = "15"
                },
                new ListeningMode
                {
                    Name = "Audyssey DSX",
                    Parameter = "16"
                },
                new ListeningMode
                {
                    Name = "Whole House Mode",
                    Parameter = "1F"
                },
                new ListeningMode
                {
                    Name = "Stage",
                    Parameter = "23"
                },
                new ListeningMode
                {
                    Name = "Action",
                    Parameter = "25"
                },
                new ListeningMode
                {
                    Name = "Music",
                    Parameter = "26"
                },
                new ListeningMode
                {
                    Name = "Sports",
                    Parameter = "2E"
                },
                new ListeningMode
                {
                    Name = "Straight Decode",
                    Parameter = "40"
                },
                new ListeningMode
                {
                    Name = "Dolby EX / DTS ES",
                    Parameter = "41"
                },
                new ListeningMode
                {
                    Name = "THX Cinema",
                    Parameter = "42"
                },
                new ListeningMode
                {
                    Name = "THX Surround EX",
                    Parameter = "43"
                },
                new ListeningMode
                {
                    Name = "THX Music",
                    Parameter = "44"
                },
                new ListeningMode
                {
                    Name = "THX Games",
                    Parameter = "45"
                },
                new ListeningMode
                {
                    Name = "THX U2/S2/I/S Cinema",
                    Parameter = "50"
                },
                new ListeningMode
                {
                    Name = "THX Music Mode, THX U2/S2/I/S Music",
                    Parameter = "51"
                },
                new ListeningMode
                {
                    Name = "THX Games Mode, THX U2/S2/I/S Games",
                    Parameter = "52"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx Movie",
                    Parameter = "80"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx Music",
                    Parameter = "81"
                },
                new ListeningMode
                {
                    Name = "Neo:X Cinema",
                    Parameter = "82"
                },
                new ListeningMode
                {
                    Name = "Neo:X Music",
                    Parameter = "83"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx THX Cinema",
                    Parameter = "84"
                },
                new ListeningMode
                {
                    Name = "Neo:X THX Cinema",
                    Parameter = "85"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx Game",
                    Parameter = "86"
                },
                new ListeningMode
                {
                    Name = "Neural Surround",
                    Parameter = "87"
                },
                new ListeningMode
                {
                    Name = "Neural THX/Neural Surround",
                    Parameter = "88"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx THX Games",
                    Parameter = "89"
                },
                new ListeningMode
                {
                    Name = "Neo:X THX Games",
                    Parameter = "8A"
                },
                new ListeningMode
                {
                    Name = "PLII/PLIIx THX Music",
                    Parameter = "8B"
                },
                new ListeningMode
                {
                    Name = "Neo:X THX Music",
                    Parameter = "8C"
                },
                new ListeningMode
                {
                    Name = "Neural THX Cinema",
                    Parameter = "8D"
                },
                new ListeningMode
                {
                    Name = "Neural THX Music",
                    Parameter = "8E"
                },
                new ListeningMode
                {
                    Name = "Neural THX Games",
                    Parameter = "8F"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height",
                    Parameter = "90"
                },
                new ListeningMode
                {
                    Name = "Neo:6 Cinema DTS Surround Sensation",
                    Parameter = "91"
                },
                new ListeningMode
                {
                    Name = "Neo:6 Music DTS Surround Sensation",
                    Parameter = "92"
                },
                new ListeningMode
                {
                    Name = "Neural Digital Music",
                    Parameter = "93"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX Cinema",
                    Parameter = "94"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX Music",
                    Parameter = "95"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX Games",
                    Parameter = "96"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX U2/S2 Cinema",
                    Parameter = "97"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX U2/S2 Music",
                    Parameter = "98"
                },
                new ListeningMode
                {
                    Name = "PLIIz Height + THX U2/S2 Games",
                    Parameter = "99"
                },
                new ListeningMode
                {
                    Name = "Neo:X Game",
                    Parameter = "9A"
                },
                new ListeningMode
                {
                    Name = "PLIIx/PLII Movie + Audyssey DSX",
                    Parameter = "A0"
                },
                new ListeningMode
                {
                    Name = "PLIIx/PLII Music + Audyssey DSX",
                    Parameter = "A1"
                },
                new ListeningMode
                {
                    Name = "PLIIx/PLII Game + Audyssey DSX",
                    Parameter = "A2"
                },
                new ListeningMode
                {
                    Name = "Neo:6 Cinema + Audyssey DSX",
                    Parameter = "A3"
                },
                new ListeningMode
                {
                    Name = "Neo:6 Music + Audyssey DSX",
                    Parameter = "A4"
                },
                new ListeningMode
                {
                    Name = "Neural Surround + Audyssey DSX",
                    Parameter = "A5"
                },
                new ListeningMode
                {
                    Name = "Neural Digital Music + Audyssey DSX",
                    Parameter = "A6"
                },
                new ListeningMode
                {
                    Name = "Dolby EX + Audyssey DSX",
                    Parameter = "A7"
                }
            };

            public static string Converter(string par)
            {
                try
                {
                    return ListeningModes.First(it => it.Parameter == par).Name;
                }
                catch (Exception)
                {
                    return "N/A";
                }
            }

            public static int ConverterToIndex(string par)
            {
                int ind = 0;
                foreach (var input in ListeningModes)
                {
                    if (input.Parameter == par)
                    {
                        return ind;
                    }
                    ind++;
                }
                return -1;
            }
        }

        public class CenterSpeaker
        {
            public const string Com = "CTL";

            public static string Request => Com + "QSTN";

            public static string Set(int pro)
            {
                string s = "";
                if (pro < 0)
                {
                    s = "-";
                    pro = pro * -1;
                }
                else if (pro > 0)
                    s = "+";
                else
                {
                    s = "0";
                }
                string r = $"{Com}{s}{pro:x1}".ToUpper();
                return r;
            }

            public static string Up => Com + "UP";
            public static string Down => Com + "DOWN";

            public static int Converter(string i)
            {
                bool n = false;
                if (i.StartsWith("-"))
                {
                    i = i.Substring(1);
                    n = true;
                }
                else if (i.StartsWith("+"))
                {
                    i = i.Substring(1);
                }
                int r = int.Parse(i, System.Globalization.NumberStyles.HexNumber);
                if (n)
                    r = 0 - r;
                return r;
            }
        }

        public class ToneFront
        {
            public const string Com = "TFR";

            public static string Request => Com + "QSTN";

            public static string SetBass(int pro)
            {
                string s = "";
                if (pro < 0)
                {
                    s = "-";
                    pro = pro * -1;
                }
                else if (pro > 0)
                    s = "+";
                else
                {
                    s = "0";
                }
                string r = $"{Com}B{s}{pro:x1}".ToUpper();
                return r;
            }

            public static string SetTreble(int pro)
            {
                string s = "";
                if (pro < 0)
                {
                    s = "-";
                    pro = pro * -1;
                }
                else if (pro > 0)
                    s = "+";
                else
                {
                    s = "0";
                }
                string r = $"{Com}T{s}{pro:x1}".ToUpper();
                return r;
            }

            public static string BassUp => Com + "BUP";
            public static string BassDown => Com + "BDOWN";

            public static string TrebleUp => Com + "TUP";
            public static string TrebleDown => Com + "TDOWN";

            public static Result Converter(string i)
            {
                var result = new Result();
                if (i.Contains("B"))
                {
                    result.Bass = true;
                    string b = i.Substring(i.IndexOf("B") + 1, 2);
                    bool n = b.StartsWith("-");
                    b = b.Substring(1);
                    int d = int.Parse(b, System.Globalization.NumberStyles.HexNumber);
                    if (n)
                        d = 0 - d;
                    result.BassVal = d;
                }
                if (i.Contains("T"))
                {
                    result.Treble = true;
                    string t = i.Substring(i.IndexOf("T") + 1, 2);
                    bool n = t.StartsWith("-");
                    t = t.Substring(1);
                    int d = int.Parse(t, System.Globalization.NumberStyles.HexNumber);
                    if (n)
                        d = 0 - d;
                    result.TrebleVal = d;
                }
                return result;
            }

            public class Result
            {
                public bool Treble;
                public int TrebleVal;
                public bool Bass;
                public int BassVal;
            }
        }

        public class AudioInformation
        {
            public const string Com = "IFA";
            public static string Request => Com + "QSTN";

            public static Result Converter(string raw)
            {
                if (raw == "N/A")
                {
                    return new Result {na = true};
                }


                var ls = raw.Split(',');
                int ind = 0;
                foreach (string s in ls)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        ls[ind] = "N/A";
                    }
                    ind++;
                }
                var res = new Result
                {
                    inputPort = ls[0].Trim(),
                    format = ls[1].Trim(),
                    freq = ls[2].Trim(),
                    channelIn = ls[3].Trim(),
                    mode = ls[4].Trim(),
                    channelOut = ls[5].Trim(),
                };
                if (res.inputPort == "NETWORK")
                {
                }

                return res;
            }

            public class Result
            {
                public bool na;
                public string inputPort, format, freq, channelIn, mode, channelOut;
            }
        }

        public class VideoInformation
        {
            public const string Com = "IFV";
            public static string Request => Com + "QSTN";

            public static Result Converter(string raw)
            {
                if (raw == "N/A")
                {
                    return new Result {na = true};
                }

                var ls = raw.Split(',');
                var res = new Result
                {
                    portIn = ls[0].Trim(),
                    resIn = ls[1].Trim(),
                    colorIn = ls[2].Trim(),
                    bitIn = ls[3].Trim(),
                    portOut = ls[4].Trim(),
                    resOut = ls[5].Trim(),
                    colorOut = ls[6].Trim(),
                    bitOut = ls[7].Trim(),
                    pictureMode = ls[8].Trim(),
                };
                return res;
            }

            public class Result
            {
                public bool na;

                public string portIn,
                    resIn,
                    freqIn,
                    colorIn,
                    bitIn,
                    portOut,
                    resOut,
                    freqOut,
                    colorOut,
                    bitOut,
                    pictureMode;
            }
        }

        public class SpeakerA
        {
            public const string Com = "SPA";
            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }

        public class SpeakerB
        {
            public const string Com = "SPB";
            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }

        public class SleepTimer
        {
            public const string Com = "SLP";

            public static string Set(int pro)
            {
                if (pro > 0)
                    return Com + $"{pro:x2}".ToUpper();
                return Disable;
            }

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public static string Disable => Com + "OFF";

            public int TimeLeft;
            public bool Active = false;

            public SleepTimer(string raw)
            {
                TimeLeft = Converter(raw);
                Active = TimeLeft > 0;
            }

            public static int Converter(string i)
            {
                return int.Parse(i, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public class SpeakerLevelCalibration
        {
            public const string Com = "SLC";

            public static string Request => Com + "QSTN";

            public static string ChSelect => Com + "CHSEL";

            public static string Up => Com + "UP";

            public static string Down => Com + "DOWN";
        }

        public class Subwoofer
        {
            public const string Com = "SWL";

            public static string Request => Com + "QSTN";

            public static string Up => Com + "UP";

            public static string Down => Com + "DOWN";
        }

        public class Dimmer
        {
            public const string Com = "DIM";

            public static string Request => Com + "QSTN";
        }

        public class HdmiOutput
        {
            public const string Com = "HDO";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";
        }

        public class HdmiAudioOutMain
        {
            public const string Com = "HAO";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";
        }

        public class HdmiAudioOutSub
        {
            public const string Com = "HAS";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";
        }

        public class HdmiCec
        {
            public const string Com = "CEC";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public bool HdmiCecEnabled;

            public HdmiCec(string raw)
            {
                HdmiCecEnabled = Converter(raw);
            }

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }

        public class MonitorOutResolution
        {
            public const string Com = "RES";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";
        }

        public class LateNight
        {
            public const string Com = "LTN";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";
        }

        public class AccuEQ
        {
            public const string Com = "AEQ";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }

        public class MusicOptimizer
        {
            public const string Com = "MOT";

            public static string Request => Com + "QSTN";

            public static string Toggle => Com + "UP";

            public static string Set(bool to)
            {
                return Com + Converter(to);
            }

            public static bool Converter(string i)
            {
                return i == "01";
            }

            public static string Converter(bool i)
            {
                return i ? "01" : "00";
            }
        }
    }
}