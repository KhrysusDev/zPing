using System;
using System.Reflection;

namespace zPing
{
    /*
     * Created by KhryusDev on GitHub
     * KhrysusDev/zPing
     * Thursday 4th March 2021, 21:22 (UTC+0)
     * Licensed under Common Development and Distribution License (CDDL)
     *
     * Hi RavelCros!
     * Enjoy this source code, because I know when you see this your 1000000% going to rip it
     * Accept it as a gift from me, but this is here so companies can see some of the stuff I can do.
     * And for all you kids that are probably going to rip it, just remember, ripping gets you nowhere in life.
     * If you rip source, its highly unlikely that you will ever learn how to actually program.
     */

    public static class Extensions
    {
        public static Version GetVersion(this Assembly assembly)
        {
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return Version.Parse(fvi.FileVersion);
        }
    }
}