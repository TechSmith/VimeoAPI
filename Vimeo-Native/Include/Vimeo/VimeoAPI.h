#ifndef VIMEOAPI_H
#define VIMEOAPI_H

#ifdef WIN32
#define VIMEO_EXTERN extern "C" __declspec(dllexport)
#else
#define VIMEO_EXTERN extern "C"
#endif

typedef wchar_t VimeoString;
typedef void* VimeoHandle;

#define VIMEO_SUCCESS               0
#define VIMEO_FAILURE_GENERIC       -1

#define VIMEO_PRIVACY_ANYBODY       0
#define VIMEO_PRIVACY_NOBODY        1
#define VIMEO_PRIVACY_CONTACTS      2
#define VIMEO_PRIVACY_PASSWORD      3

class IVimeoProgress
{
public:
   virtual void SetProgress(double dPercent)    = 0;
   virtual bool GetCanceled()                   = 0;
};

typedef int (*PVIMEO_CREATE_FUNC)                        (VimeoHandle* ppHandle, const VimeoString* pstrClientID, const VimeoString* pstrClientSecret);
typedef int (*PVIMEO_FREE_FUNC)                          (VimeoHandle* ppHandle);
typedef int (*PVIMEO_LOADAUTHORIZATIONTOKEN_FUNC)        (VimeoHandle pHandle, const VimeoString* pstrAuthToken);
typedef int (*PVIMEO_GETAUTHORIZATIONURL_FUNC)           (VimeoHandle pHandle, VimeoString* pstrURL, int& nSizeOfURL);
typedef int (*PVIMEO_GETUSERNAME_FUNC)                   (VimeoHandle pHandle, VimeoString* pstrUserName, int& nSizeOfUserName);

typedef int (*PVIMEO_GETURL_FUNC)                        (VimeoHandle pHandle, VimeoString *pstrURL, int& nSizeOfURL);
typedef int (*PVIMEO_ISCALLBACKURL_FUNC)                 (const VimeoString* pstrURL);
typedef int (*PVIMEO_OBTAINACCESSTOKEN_FUNC)             (VimeoHandle pHandle, VimeoString* pstrURL);
typedef int (*PVIMEO_GETACCESSTOKEN_FUNC)                (VimeoHandle pHandle, VimeoString *pstrAccessToken, int& nSizeOfAccessToken);

typedef int (*PVIMEO_UPLOADFILE_FUNC)                    (VimeoHandle pHandle, const VimeoString* pstrPath, const VimeoString* pstrTitle, const VimeoString* pstrDescription, const VimeoString* pstrTags, int nPrivacy, const VimeoString* pstrPrivacy, IVimeoProgress* pProgress);
typedef int (*PVIMEO_GETERRORMESSAGE_FUNC)               (VimeoHandle pHandle, VimeoString* pstrMessage, int& nSizeOfMessage);

VIMEO_EXTERN int VimeoCreate(VimeoHandle* ppHandle, const VimeoString* pstrClientID, const VimeoString* pstrClientSecret);
VIMEO_EXTERN int VimeoFree(VimeoHandle* ppHandle);
VIMEO_EXTERN int VimeoLoadAccessToken(VimeoHandle pHandle, const VimeoString* pstrAuthToken);
VIMEO_EXTERN int VimeoGetUserAuthorizationURL(VimeoHandle pHandle, VimeoString* pstrURL, int& nSizeOfURL);
VIMEO_EXTERN int VimeoGetUserName(VimeoHandle pHandle, VimeoString* pstrUserName, int& nSizeOfUserName);

VIMEO_EXTERN int VimeoGetURL(VimeoHandle pHandle, VimeoString *pstrURL, int& nSizeOfURL);
VIMEO_EXTERN int VimeoIsCallbackURL(const VimeoString* pstrURL);
VIMEO_EXTERN int VimeoObtainAccessToken(VimeoHandle pHandle, VimeoString* pstrURL);
VIMEO_EXTERN int VimeoGetAccessToken(VimeoHandle pHandle, VimeoString *pstrAccessToken, int& nSizeOfAccessToken);

VIMEO_EXTERN int VimeoUploadFile(VimeoHandle pHandle, const VimeoString* pstrPath, const VimeoString* pstrTitle, const VimeoString* pstrDescription, const VimeoString* pstrTags, int nPrivacy, const VimeoString* pstrPrivacy, IVimeoProgress* pProgress);
VIMEO_EXTERN int VimeoGetErrorMessage(VimeoHandle pHandle, VimeoString* pstrMessage, int& nSizeOfMessage);

#endif
