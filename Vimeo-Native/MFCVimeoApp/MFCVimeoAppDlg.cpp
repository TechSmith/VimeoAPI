
// MFCVimeoAppDlg.cpp : implementation file
//

#include "stdafx.h"
#include "MFCVimeoApp.h"
#include "MFCVimeoAppDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialogEx
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialogEx(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialogEx)
END_MESSAGE_MAP()


// CMFCVimeoAppDlg dialog




CMFCVimeoAppDlg::CMFCVimeoAppDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CMFCVimeoAppDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CMFCVimeoAppDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CMFCVimeoAppDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
   ON_BN_CLICKED(IDC_BTN_UPLOAD, &CMFCVimeoAppDlg::OnBnClickedBtnUpload)
END_MESSAGE_MAP()


// CMFCVimeoAppDlg message handlers

BOOL CMFCVimeoAppDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

   SetDlgItemText(IDC_EDIT_ACCESSTOKEN, _T("93d903eab03d35ef074160d0ecc5fc54"));//No guarantee that this will work when you try it :)
	SetDlgItemText(IDC_EDIT_UPLOAD_FILE, _T("C:\\green.avi"));

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CMFCVimeoAppDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialogEx::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CMFCVimeoAppDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CMFCVimeoAppDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

#ifndef ARR_SIZE
#define ARR_SIZE(x) (sizeof(x)/sizeof(x[0]))
#endif

void CMFCVimeoAppDlg::OnBnClickedBtnUpload()
{
   CString strAccessToken, strFile, strTitle, strDescription, strTags, strPassword;
   struct IDAndString
   {
      UINT uid;
      CString& str;
   } arrIDtoSTR[] = 
   {
      { IDC_EDIT_ACCESSTOKEN, strAccessToken},
      { IDC_EDIT_UPLOAD_FILE, strFile},
      { IDC_EDIT_TITLE,       strTitle},
      { IDC_EDIT_DESCRIPTION, strDescription},
      { IDC_EDIT_TAGS,        strTags},
   };

   for(int i=0; i<ARR_SIZE(arrIDtoSTR); i++)
   {
      GetDlgItemText(arrIDtoSTR[i].uid,   arrIDtoSTR[i].str);
      if( arrIDtoSTR[i].str.IsEmpty() )
      {
         AfxMessageBox(_T("Must fill out all items"));
         GetDlgItem(arrIDtoSTR[i].uid)->SetFocus();
         return;
      }
   }

   VimeoHandle vimeo;
   int nSizeOfURL = 0;
   if( VIMEO_SUCCESS != VimeoCreate(&vimeo, 
      _T("5f53d7b66f4f13e0d96ccbf39d699fb98571b6a9"),
      _T("q8NnGuIUCqexanswFpa4f8eyluDo2a/HSKoQlDf8BRbFUL7Rb+zRwpoDSVKR73Vhw4X7XFMNfT+N4sw/OSZ5glrElCYuKhbtqvTXoFbOLO/q8zisMT7XhOo+tyjZ/KQr") ) )
   {
      AfxMessageBox(_T("Failure to create the Vimeo object"));
      return;
   }

   int nRes = VimeoLoadAccessToken(vimeo, strAccessToken.LockBuffer());
   strAccessToken.UnlockBuffer();
   if( VIMEO_SUCCESS != nRes )
   {
      AfxMessageBox(_T("Failure to load access token"));
      goto Free;
   }

   nRes = VimeoUploadFile(vimeo, 
      strFile.LockBuffer(), 
      strTitle.LockBuffer(), 
      strDescription.LockBuffer(), 
      strTags.LockBuffer(), 
      VIMEO_PRIVACY_ANYBODY,
      strPassword.LockBuffer(),
      NULL);
   strFile.UnlockBuffer();
   strTitle.UnlockBuffer();
   strDescription.UnlockBuffer();
   strTags.UnlockBuffer();
   strPassword.UnlockBuffer();
   if( VIMEO_SUCCESS != nRes )
   {
      AfxMessageBox(_T("Failure to upload the file"));
      goto Free;
   }

   VimeoGetURL(vimeo, NULL, nSizeOfURL);

Free:
   VimeoFree(&vimeo);
}
