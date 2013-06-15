using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    class Version {

        public static Version Unknown = new Version(0xff, 0xfe);
        public static Version Blank = new Version(0xff, 0xff);

        public byte Major { get; private set; }
        public byte Minor { get; private set; }

        public Version(byte major, byte minor) {
            Major = major;
            Minor = minor;
        }

        public override int GetHashCode() {
            return Major * 256 + Minor;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((Version) obj);
        }

        protected bool Equals(Version v) {
            return Major == v.Major && Minor == v.Minor;
        }

        public override string ToString() {
            if(Equals(Unknown))
                return "unknown/corrupted";
            if(Equals(Blank))
                return "blank";
            return string.Format("{0}.{1}", Major, Minor.ToString("000"));
        }

        public static Version Parse(string s) {
            s = s.ToLower();
            if(s == "unknown")
                return Unknown;
            if(s == "blank")
                return Blank;

            var parts = s.Split('.');
            return new Version(byte.Parse(parts[0]), byte.Parse(parts[1]));
        }
    }
}