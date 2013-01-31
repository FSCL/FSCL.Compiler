///////////////////////////////////////////////////////////////////////////////
//
//  Silverlight.supportedUserAgent.js   	version 2.0.30822.0
//
//  This file is provided by Microsoft as a helper file for websites that
//  incorporate Silverlight Objects. This file is provided under the Microsoft
//  Public License available at 
//  http://code.msdn.microsoft.com/SLsupportedUA/Project/License.aspx.  
//  You may not use or distribute this file or the code in this file except as 
//  expressly permitted under that license.
// 
//  Copyright (c) Microsoft Corporation. All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

if (!window.Silverlight)
{
    window.Silverlight = { };
}

///////////////////////////////////////////////////////////////////////////////
//
// supportedUserAgent:
//
// NOTE: This function is strongly tied to current implementations of web 
// browsers. The implementation of this function will change over time to 
// account for new Web browser developments. Visit 
// http://code.msdn.microsoft.com/SLsupportedUA often to ensure that you have
// the latest version.
//
// Determines if the client browser is supported by Silverlight. 
//
//  params:
//   version [string] 
//         determines if a particular version of Silverlight supports
//         this browser. Acceptable values are "1.0" and "2.0"
//   userAgent [string]
//         optional. User Agent string to be analized. If null then the
//         current browsers user agent string will be used.
//
//  return value: boolean
//
///////////////////////////////////////////////////////////////////////////////
Silverlight.supportedUserAgent = function(version, userAgent)
{
    try
    {
        var ua = null;

        if ( userAgent)
        {
           ua = userAgent;
        }
        else
        {
           ua = window.navigator.userAgent;
        }
        
        var slua = {OS:'Unsupported',Browser:'Unsupported'};
        
        //Silverlight does not support pre-Windows NT platforms
        if (ua.indexOf('Windows NT') >= 0 || ua.indexOf('Mozilla/4.0 (compatible; MSIE 6.0)')>=0) {
            slua.OS = 'Windows';
        }
        else if (ua.indexOf('PPC Mac OS X') >= 0) {
            slua.OS = 'MacPPC';
        }
        else if (ua.indexOf('Intel Mac OS X') >= 0) {
            slua.OS = 'MacIntel';
        }
        
        if ( slua.OS != 'Unsupported' )
        {
            if (ua.indexOf('MSIE') >= 0) {
                if (navigator.userAgent.indexOf('Win64') == -1)
                {
                    if (parseInt(ua.split('MSIE')[1]) >= 6) {
                        slua.Browser  = 'MSIE';
                    }
                }
            }
            else if (ua.indexOf('Firefox') >= 0) {
                var versionArr = ua.split('Firefox/')[1].split('.');
                var major = parseInt(versionArr[0]);
                if (major >= 2) {
                    slua.Browser = 'Firefox';
                }
                else {
                    var minor = parseInt(versionArr[1]);
                    if ((major == 1) && (minor >= 5)) {
                        slua.Browser  = 'Firefox';
                    }
                }
            }
            
            else if (ua.indexOf('Safari') >= 0) {
                slua.Browser = 'Safari';
            }            
        }
        
        //detect all unsupported platform combinations (IE on Mac, Safari on Win)
        var supUA =   (!(   slua.OS == 'Unsupported' ||                             //Unsupported OS
                            slua.Browser == 'Unsupported' ||                        //Unsupported Browser
                            (slua.OS == 'Windows' && slua.Browser == 'Safari') ||   //Safari is not supported on Windows
                            (slua.OS.indexOf('Mac') >= 0 && slua.Browser == 'MSIE')   //IE is not supported on Mac
                                ));

        if (version=='2.0')
        {
            //add PPC to unsupported list
            return (supUA && (slua.OS != 'MacPPC' ));
        }
        else if (version == '1.0')
        {
            //add win2k to unsupported list
            return (supUA && ( ua.indexOf('Windows NT 5.0') < 0));
        }
        else
        {
            return (supUA);  
        }  
    }
    catch (e)
    {
        return false;
    }
}
