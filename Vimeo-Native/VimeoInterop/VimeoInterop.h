// VimeoInterop.h

#pragma once

using namespace System;

#include "../Include/Vimeo/VimeoAPI.h"

#define VIMEO_EXTRA_SAFE

#ifdef VIMEO_EXTRA_SAFE
#define GET_LIB(pHandle)      GlobalVimeoObjs::_GCLibMap->ContainsKey( *((int*)pHandle) ) ? GlobalVimeoObjs::_GCLibMap[*((int*)pHandle)] : nullptr;
#define CHECK_LIB(pLib)       { if( pLib == nullptr ) return VIMEO_FAILURE_GENERIC; }
#else
#define GET_LIB(pHandle)      GlobalVimeoObjs::_GCLibMap[*((int*)pHandle)];
#define CHECK_LIB(pLib)
#endif

ref class GlobalVimeoObjs {
public:
   static System::Collections::Generic::IDictionary<int, VimeoAPI::Vimeo^> ^ _GCLibMap = gcnew System::Collections::Generic::Dictionary<int, VimeoAPI::Vimeo^>();
};

VIMEO_EXTERN int VimeoCreate(VimeoHandle* ppHandle, VimeoString* pstrClientID, VimeoString* pstrClientSecret)
{
   String ^strClientID     = gcnew String(pstrClientID);
   String ^strClientSecret = gcnew String(pstrClientSecret);
   VimeoAPI::Vimeo^ pLib = gcnew VimeoAPI::Vimeo(strClientID, strClientSecret);
   int* pInt = new int;
   System::Random^ random = gcnew System::Random();
   do
   {
      *pInt = random->Next();
   } while( GlobalVimeoObjs::_GCLibMap->ContainsKey(*pInt) );
   *ppHandle = (void*)pInt;
   GlobalVimeoObjs::_GCLibMap[*pInt] = pLib;
   return VIMEO_SUCCESS;
}

VIMEO_EXTERN int VimeoFree(VimeoHandle* ppHandle)
{
   int* pInt = (int*)*ppHandle;
   GlobalVimeoObjs::_GCLibMap->Remove(*pInt);
   delete pInt;

   return VIMEO_SUCCESS;
}

VIMEO_EXTERN int VimeoLoadAccessToken(VimeoHandle pHandle, VimeoString* pstrAuthToken)
{
   String ^strAuthToken     = gcnew String(pstrAuthToken);
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   bool bOK = pVimeo->LoadAccessToken(strAuthToken);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

#include <string.h>
#include<vcclr.h>
void VimeoTwoStepRequiredStringSave(System::String^% Str, VimeoString* pstr, int& nSizeOfStr)
{
   if( nSizeOfStr > 0 )
   {
      cli::pin_ptr<const System::Char> pChar = PtrToStringChars(Str);
      const wchar_t *psz = pChar;
      memcpy( pstr, psz, (nSizeOfStr + 1) * sizeof(VimeoString) );
   }
   else
   {
      nSizeOfStr = Str->Length;
   }
}

VIMEO_EXTERN int VimeoGetUserAuthorizationURL(VimeoHandle pHandle, VimeoString* pstrURL, int& nSizeOfURL)
{
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   String^ strURL;
   bool bOK = pVimeo->GetUserAuthorizationURL(strURL);

   VimeoTwoStepRequiredStringSave(strURL, pstrURL, nSizeOfURL);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoGetUserName(VimeoHandle pHandle, VimeoString* pstrUserName, int& nSizeOfUserName)
{
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   String^ strUsername;
   bool bOK = pVimeo->GetUserName(strUsername);

   VimeoTwoStepRequiredStringSave(strUsername, pstrUserName, nSizeOfUserName);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoGetURL(VimeoHandle pHandle, VimeoString *pstrURL, int& nSizeOfURL)
{
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   String^ strURL;
   bool bOK = pVimeo->GetURL(strURL);

   VimeoTwoStepRequiredStringSave(strURL, pstrURL, nSizeOfURL);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoIsCallbackURL(VimeoString* pstrURL)
{
   String ^strURL     = gcnew String(pstrURL);
   bool bOK = VimeoAPI::Vimeo::StartsWithCallbackURL(strURL);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoObtainAccessToken(VimeoHandle pHandle, VimeoString* pstrURL)
{
   String ^strURL     = gcnew String(pstrURL);

   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);

   bool bOK = pVimeo->ObtainAccessToken(strURL);
   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoGetAccessToken(VimeoHandle pHandle, VimeoString *pstrAccessToken, int& nSizeOfAccessToken)
{
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   String^ strAccessToken;
   bool bOK = pVimeo->GetAccessToken(strAccessToken);

   VimeoTwoStepRequiredStringSave(strAccessToken, pstrAccessToken, nSizeOfAccessToken);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

public ref class IVimeoProgressAdapter: public VimeoAPI::IProgress
{
public:
   IVimeoProgressAdapter(IVimeoProgress* pProgress)
      : m_pProgress(pProgress)
   {

   }

   virtual void SetProgress(double dPercent);

   virtual System::Boolean GetCanceled();

protected:
   IVimeoProgress* m_pProgress;
};

inline void IVimeoProgressAdapter::SetProgress(double dPercent)
{
   if( m_pProgress )
      m_pProgress->SetProgress(dPercent);
}

inline System::Boolean IVimeoProgressAdapter::GetCanceled()
{
   return m_pProgress ? m_pProgress->GetCanceled() : false;
}

VIMEO_EXTERN int VimeoUploadFile(VimeoHandle pHandle, VimeoString* pstrPath, VimeoString* pstrTitle, VimeoString* pstrDescription, VimeoString* pstrTags, int nPrivacy, VimeoString* pstrPassword, IVimeoProgress* pProgress)
{
   String ^strPath         = gcnew String(pstrPath);
   String ^strTitle        = gcnew String(pstrTitle);
   String ^strDescription  = gcnew String(pstrDescription);
   String ^strTags         = gcnew String(pstrTags);
   String ^strPassword     = gcnew String(pstrPassword);
   VimeoAPI::Vimeo::Privacy ePrivacy = (VimeoAPI::Vimeo::Privacy)nPrivacy;
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);

   VimeoAPI::IProgress^ pProg = gcnew IVimeoProgressAdapter(pProgress);

   bool bOK = pVimeo->Upload(strPath, strTitle, strDescription, strTags, ePrivacy, strPassword, pProg);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

VIMEO_EXTERN int VimeoGetErrorMessage(VimeoHandle pHandle, VimeoString* pstrMessage, int& nSizeOfMessage)
{
   VimeoAPI::Vimeo^ pVimeo = GET_LIB(pHandle);
   CHECK_LIB(pVimeo);
   String^ strErrorMessage;
   bool bOK = pVimeo->GetErrorMessage(strErrorMessage);

   VimeoTwoStepRequiredStringSave(strErrorMessage, pstrMessage, nSizeOfMessage);

   return bOK ? VIMEO_SUCCESS : VIMEO_FAILURE_GENERIC;
}

