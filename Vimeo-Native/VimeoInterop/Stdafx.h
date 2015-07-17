// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef NEVER_TRANSLATE
#define NEVER_TRANSLATE(x) _T(x)
#endif
#ifndef NEVER_TRANSLATEA
#define NEVER_TRANSLATEA(x)   x
#endif
#ifndef NEVER_TRANSLATEL
#define NEVER_TRANSLATEL(x) L ## x
#endif

#ifdef _DEBUG
#define ASSERT(x) { if(!(x)) System::Diagnostics::Debug::Assert(false); }
#else
#define ASSERT(x) {(void)(x);}
#endif

