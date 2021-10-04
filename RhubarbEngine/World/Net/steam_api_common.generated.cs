//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

namespace Steam
{
    using System.Runtime.InteropServices;
    
    public static partial class Networking
    {
        public enum __AnonymousCppEnum_steam_api_common_290AnonymousEnum : int
        {
            Isteamnetworkingsocketscallbacks = unchecked((int)1220),
        }
        
        public const Networking.__AnonymousCppEnum_steam_api_common_290AnonymousEnum Isteamnetworkingsocketscallbacks = __AnonymousCppEnum_steam_api_common_290AnonymousEnum.Isteamnetworkingsocketscallbacks;
        
        public enum __AnonymousCppEnum_steam_api_common_343AnonymousEnum : int
        {
            Isteamnetworkingmessagescallbacks = unchecked((int)1250),
        }
        
        public const Networking.__AnonymousCppEnum_steam_api_common_343AnonymousEnum Isteamnetworkingmessagescallbacks = __AnonymousCppEnum_steam_api_common_343AnonymousEnum.Isteamnetworkingmessagescallbacks;
        
        public enum __AnonymousCppEnum_steam_api_common_397AnonymousEnum : int
        {
            Isteamnetworkingutilscallbacks = unchecked((int)1280),
        }
        
        public const Networking.__AnonymousCppEnum_steam_api_common_397AnonymousEnum Isteamnetworkingutilscallbacks = __AnonymousCppEnum_steam_api_common_397AnonymousEnum.Isteamnetworkingutilscallbacks;
        
        /// <summary>
        /// handle to a communication pipe to the Steam client
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public readonly partial struct HSteamPipe : IEquatable<HSteamPipe>
        {
            public HSteamPipe(int value) => this.Value = value;
            
            public readonly int Value;
            
            public bool Equals(HSteamPipe other) =>  Value.Equals(other.Value);
            
            public override bool Equals(object obj) => obj is HSteamPipe other && Equals(other);
            
            public override int GetHashCode() => Value.GetHashCode();
            
            public override string ToString() => Value.ToString();
            
            public static implicit operator int(HSteamPipe from) => from.Value;
            
            public static implicit operator HSteamPipe(int from) => new HSteamPipe(from);
            
            public static bool operator ==(HSteamPipe left, HSteamPipe right) => left.Equals(right);
            
            public static bool operator !=(HSteamPipe left, HSteamPipe right) => !left.Equals(right);
        }
        
        /// <summary>
        /// handle to single instance of a steam user
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public readonly partial struct HSteamUser : IEquatable<HSteamUser>
        {
            public HSteamUser(int value) => this.Value = value;
            
            public readonly int Value;
            
            public bool Equals(HSteamUser other) =>  Value.Equals(other.Value);
            
            public override bool Equals(object obj) => obj is HSteamUser other && Equals(other);
            
            public override int GetHashCode() => Value.GetHashCode();
            
            public override string ToString() => Value.ToString();
            
            public static implicit operator int(HSteamUser from) => from.Value;
            
            public static implicit operator HSteamUser(int from) => new HSteamUser(from);
            
            public static bool operator ==(HSteamUser left, HSteamUser right) => left.Equals(right);
            
            public static bool operator !=(HSteamUser left, HSteamUser right) => !left.Equals(right);
        }
    }
}
