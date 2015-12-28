// geiForm.h
//  o contains form description for Graphic Engine's GUI
//  o geiForm.resx needed for Design view
//  o compiles into a static library (along with geiForm.cpp)
//
//  o history
//    -v1 20070302 bakbal - original Form
//    -v2 20070428 ekhan  - add getter and setter for session filename 
//    -v3 20070519 ekhan  - increase TextBox size (full pathname to EVS .session file is long)

#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Drawing;


namespace libgeigui {

	public ref class geiForm : public System::Windows::Forms::Form
	{
	public:
		geiForm(void)
		{
			InitializeComponent();
		}

	protected:
		~geiForm()
		{
			if (components)
			{
				delete components;
			}
		}

	private: 
		System::Windows::Forms::GroupBox^  groupBox1;
		System::Windows::Forms::Button^  browseSessionButton;
		System::Windows::Forms::Label^  selectSessionLabel;
		System::Windows::Forms::Button^  startSessionButton;
		System::Windows::Forms::TextBox^  selectSessionBox;
		System::Windows::Forms::OpenFileDialog^  openSessionFileDialog;
		System::IO::StreamReader ^ sessionFile;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

		static String^ sfilename;	// holds session file name for retrieval by doGUI()
		
	public:
		property static String^ Sfilename
		{
			String^ get()
			{
				return sfilename;
			}
			void set(String^ value)
			{
				sfilename=gcnew String(value);
			}
		}

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->groupBox1 = (gcnew System::Windows::Forms::GroupBox());
			this->startSessionButton = (gcnew System::Windows::Forms::Button());
			this->selectSessionBox = (gcnew System::Windows::Forms::TextBox());
			this->browseSessionButton = (gcnew System::Windows::Forms::Button());
			this->selectSessionLabel = (gcnew System::Windows::Forms::Label());
			this->openSessionFileDialog = (gcnew System::Windows::Forms::OpenFileDialog());
			this->groupBox1->SuspendLayout();
			this->SuspendLayout();
			// 
			// groupBox1
			// 
			this->groupBox1->Controls->Add(this->startSessionButton);
			this->groupBox1->Controls->Add(this->selectSessionBox);
			this->groupBox1->Controls->Add(this->browseSessionButton);
			this->groupBox1->Controls->Add(this->selectSessionLabel);
			this->groupBox1->Location = System::Drawing::Point(31, 26);
			this->groupBox1->Name = L"groupBox1";
			this->groupBox1->Size = System::Drawing::Size(491, 191);
			this->groupBox1->TabIndex = 0;
			this->groupBox1->TabStop = false;
			this->groupBox1->Text = L"Conduct Experiment";
			// 
			// startSessionButton
			// 
			this->startSessionButton->Location = System::Drawing::Point(86, 94);
			this->startSessionButton->Name = L"startSessionButton";
			this->startSessionButton->Size = System::Drawing::Size(90, 23);
			this->startSessionButton->TabIndex = 3;
			this->startSessionButton->Text = L"Start Session";
			this->startSessionButton->UseVisualStyleBackColor = true;
			// 
			// selectSessionBox
			// 
			this->selectSessionBox->Location = System::Drawing::Point(86, 47);
			this->selectSessionBox->Name = L"selectSessionBox";
			this->selectSessionBox->Size = System::Drawing::Size(318, 20);
			this->selectSessionBox->TabIndex = 2;
			// 
			// browseSessionButton
			// 
			this->browseSessionButton->Location = System::Drawing::Point(410, 46);
			this->browseSessionButton->Name = L"browseSessionButton";
			this->browseSessionButton->Size = System::Drawing::Size(75, 23);
			this->browseSessionButton->TabIndex = 1;
			this->browseSessionButton->Text = L"Browse";
			this->browseSessionButton->UseVisualStyleBackColor = true;
			this->browseSessionButton->Click += gcnew System::EventHandler(this, &geiForm::browseSessionButton_Click);
			// 
			// selectSessionLabel
			// 
			this->selectSessionLabel->AutoSize = true;
			this->selectSessionLabel->Location = System::Drawing::Point(6, 50);
			this->selectSessionLabel->Name = L"selectSessionLabel";
			this->selectSessionLabel->Size = System::Drawing::Size(80, 13);
			this->selectSessionLabel->TabIndex = 0;
			this->selectSessionLabel->Text = L"Select Session:";
			this->startSessionButton->Click += gcnew System::EventHandler(this, &geiForm::startSessionButton_Click);
			// 
			// openSessionFileDialog
			// 
			this->openSessionFileDialog->FileName = L"";
			// 
			// geiForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(552, 247);
			this->Controls->Add(this->groupBox1);
			this->MaximumSize = System::Drawing::Size(560, 278);
			this->Name = L"geiForm";
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"Graphics Engine";
			this->groupBox1->ResumeLayout(false);
			this->groupBox1->PerformLayout();
			this->ResumeLayout(false);

		}
#pragma endregion

		private: 
		void browseSessionButton_Click(Object^ /*sender*/, System::EventArgs^ /*e*/)
		{
			openSessionFileDialog->InitialDirectory = "C:\\EVS\\experiments\\" ;
			openSessionFileDialog->Filter = "session files (*.session)|*.session|xml files (*.xml)|*.xml|All files (*.*)|*.*" ;
			openSessionFileDialog->FilterIndex = 1 ;
			openSessionFileDialog->RestoreDirectory = true ;

			if(openSessionFileDialog->ShowDialog() == Windows::Forms::DialogResult::OK)
			{
				sessionFile = gcnew System::IO::StreamReader(openSessionFileDialog->FileName);

				// Insert the selected Session file name into the text box
				selectSessionBox->Text = openSessionFileDialog->FileName;
			}
		}

		private: 
		System::Void startSessionButton_Click(System::Object^  sender, System::EventArgs^  e) 
		{ 
			// check if a value is assigned to sessionFile
			if(sessionFile == nullptr) {
				MessageBox::Show("You should first select a session file.", "Session File Selection Warning", MessageBoxButtons::OK, MessageBoxIcon::Warning);
			}
			else
			{
				Sfilename::set(selectSessionBox->Text);
				Close();
			}
		}
	};
}
