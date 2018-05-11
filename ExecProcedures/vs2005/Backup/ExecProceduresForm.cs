// HDevEngine/.NET (C#) example for executing local and external HDevelop procedures
//
//© 2007-2017 MVTec Software GmbH
//
// Purpose:
// This example program shows how the classes HDevEngine, HDevProcedureCall,
// and HDevOpMultiWindowImpl can be used in order to implement a fin detection application.
// It uses the local and external procedures contained and referenced in the
// HDevelop program fin_detection.hdev, which can be found in the
// directory hdevelop.
// When you click the button Load, the HDevelop program is loaded, the other buttons
// execute procedures that initialize image acquisition, grab and process images,
// and visualize details, respectively. For the latter, the class HDevOpMultiWindowImpl 
// is used, which implements HDevelop's internal display operators.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using HalconDotNet;

namespace ExecProcedures
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class ExecProceduresForm : System.Windows.Forms.Form
  {
    internal System.Windows.Forms.Button LoadBtn;
    internal System.Windows.Forms.Button InitAcqBtn;
    internal System.Windows.Forms.Button ProcessImageBtn;
    internal System.Windows.Forms.Button VisualizeDetailsBtn;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    // HDevEngine
    // instance of the engine
    private HDevEngine MyEngine = new HDevEngine();
    // path of HDevelop program
    String  ProgramPathString;
    // procedure calls
    private HDevProcedureCall InitAcqProcCall;
    private HDevProcedureCall ProcessImageProcCall;
    private HDevProcedureCall VisualizeDetailsProcCall;
    // implementation of the display operators
    private HDevOpMultiWindowImpl MyHDevOperatorImpl;
    // HALCON window
    private HWindow Window;
    // image acquisition device and image size
    HFramegrabber Framegrabber;
    // image and extracted region
    HImage        Image = new HImage();
    private HSmartWindowControl WindowControl;
    HRegion       FinRegion = new HRegion();

    public ExecProceduresForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if (components != null) 
        {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.LoadBtn = new System.Windows.Forms.Button();
      this.InitAcqBtn = new System.Windows.Forms.Button();
      this.ProcessImageBtn = new System.Windows.Forms.Button();
      this.VisualizeDetailsBtn = new System.Windows.Forms.Button();
      this.WindowControl = new HalconDotNet.HSmartWindowControl();
      this.SuspendLayout();
      // 
      // LoadBtn
      // 
      this.LoadBtn.Location = new System.Drawing.Point(424, 8);
      this.LoadBtn.Name = "LoadBtn";
      this.LoadBtn.Size = new System.Drawing.Size(120, 40);
      this.LoadBtn.TabIndex = 4;
      this.LoadBtn.Text = "Load Program";
      this.LoadBtn.Click += new System.EventHandler(this.LoadBtn_Click);
      // 
      // InitAcqBtn
      // 
      this.InitAcqBtn.Location = new System.Drawing.Point(424, 64);
      this.InitAcqBtn.Name = "InitAcqBtn";
      this.InitAcqBtn.Size = new System.Drawing.Size(120, 40);
      this.InitAcqBtn.TabIndex = 7;
      this.InitAcqBtn.Text = "Initialize Acquisition";
      this.InitAcqBtn.Click += new System.EventHandler(this.InitAcqBtn_Click);
      // 
      // ProcessImageBtn
      // 
      this.ProcessImageBtn.Location = new System.Drawing.Point(424, 120);
      this.ProcessImageBtn.Name = "ProcessImageBtn";
      this.ProcessImageBtn.Size = new System.Drawing.Size(120, 40);
      this.ProcessImageBtn.TabIndex = 8;
      this.ProcessImageBtn.Text = "Process Image";
      this.ProcessImageBtn.Click += new System.EventHandler(this.ProcessImageBtn_Click);
      // 
      // VisualizeDetailsBtn
      // 
      this.VisualizeDetailsBtn.Location = new System.Drawing.Point(424, 176);
      this.VisualizeDetailsBtn.Name = "VisualizeDetailsBtn";
      this.VisualizeDetailsBtn.Size = new System.Drawing.Size(120, 40);
      this.VisualizeDetailsBtn.TabIndex = 9;
      this.VisualizeDetailsBtn.Text = "Visualize Details";
      this.VisualizeDetailsBtn.Click += new System.EventHandler(this.VisualizeDetailsBtn_Click);
      // 
      // WindowControl
      // 
      this.WindowControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.WindowControl.HImagePart = new System.Drawing.Rectangle(0, 0, 768, 576);
      this.WindowControl.Location = new System.Drawing.Point(16, 8);
      this.WindowControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.WindowControl.Name = "WindowControl";
      this.WindowControl.Size = new System.Drawing.Size(384, 288);
      this.WindowControl.TabIndex = 10;
      this.WindowControl.WindowSize = new System.Drawing.Size(384, 288);
      this.WindowControl.Load += new System.EventHandler(this.WindowControl_Load);
      // 
      // ExecProceduresForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(560, 309);
      this.Controls.Add(this.WindowControl);
      this.Controls.Add(this.VisualizeDetailsBtn);
      this.Controls.Add(this.ProcessImageBtn);
      this.Controls.Add(this.InitAcqBtn);
      this.Controls.Add(this.LoadBtn);
      this.Name = "ExecProceduresForm";
      this.Text = "Execute Local and External HDevelop Procedures via HDevEngine";
      this.Load += new System.EventHandler(this.ExecProceduresForm_Load);
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
      Application.Run(new ExecProceduresForm());
    }

    private void ExecProceduresForm_Load(object sender, System.EventArgs e)
    {
      // path of external procedures
      string halconExamples = HSystem.GetSystem("example_dir");
      string ProcedurePath = halconExamples + @"\hdevengine\procedures";

      ProgramPathString = halconExamples +
      @"\hdevengine\hdevelop\fin_detection.hdev";
      if (!HalconAPI.isWindows)
        {
      // Unix-based systems (Mono)
      ProcedurePath = ProcedurePath.Replace('\\', '/');
      ProgramPathString = ProgramPathString.Replace('\\', '/');
      }
      MyEngine.SetProcedurePath(ProcedurePath);
        // disable buttons
        InitAcqBtn.Enabled = false;
        ProcessImageBtn.Enabled = false;
        VisualizeDetailsBtn.Enabled = false;
    }

    private void LoadBtn_Click(object sender, System.EventArgs e)
    {
      // load program and access procedure calls
      try
      {
        HDevProgram Program = new HDevProgram(ProgramPathString);

        HDevProcedure InitAcqProc = new HDevProcedure(Program, "init_acquisition");
        HDevProcedure ProcessImageProc = new HDevProcedure(Program, "detect_fin");
        HDevProcedure VisualizeDetailsProc = 
              new HDevProcedure(Program, "display_zoomed_region");

        InitAcqProcCall = new HDevProcedureCall(InitAcqProc);
        ProcessImageProcCall = new HDevProcedureCall(ProcessImageProc);
        VisualizeDetailsProcCall = new HDevProcedureCall(VisualizeDetailsProc);
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
        return;
      }

      // enable InitAcq button and disable Load button
      InitAcqBtn.Enabled = true;
      LoadBtn.Enabled = false;
    }

    private void InitAcqBtn_Click(object sender, System.EventArgs e)
    {
      try
      {
        // execute procedure
        InitAcqProcCall.Execute();
        // get output parameters from procedure call
        Framegrabber = 
            new HFramegrabber(InitAcqProcCall.GetOutputCtrlParamTuple("AcqHandle"));
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
        return;
      }

      HImage  Image = Framegrabber.GrabImage();
      Image.DispObj(Window);
      Image.Dispose();
      
      // enable ProcessImage button and disable InitAcq button
      ProcessImageBtn.Enabled = true;
      InitAcqBtn.Enabled = false;
    }

    private void ProcessImageBtn_Click(object sender, System.EventArgs e)
    {
      // image processing variables
      HTuple       FinArea;

      // free memory of iconic results of previous execution
      Image.Dispose();
      FinRegion.Dispose();
      // read image and process it
      Image = Framegrabber.GrabImage();
      Image.DispObj(Window);
      try
      {
        // execute procedure
        ProcessImageProcCall.SetInputIconicParamObject("Image", Image);
        ProcessImageProcCall.Execute();
        // get output parameters from procedure call
        FinRegion = ProcessImageProcCall.GetOutputIconicParamRegion("FinRegion");
        FinArea = ProcessImageProcCall.GetOutputCtrlParamTuple("FinArea");
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
        return;
      }
      // display results
      Image.DispObj(Window);
      Window.SetColor("red");
      Window.DispObj(FinRegion);
      Window.SetColor("white");
      Window.SetTposition(150, 20);
      Window.WriteString("Fin Area: " + FinArea.D);

      // enable VisualizeDetails button
      VisualizeDetailsBtn.Enabled = true;
    }

    private void VisualizeDetailsBtn_Click(object sender, System.EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      try
      {
        // enable HDevelop's display operators
        MyEngine.SetHDevOperators(MyHDevOperatorImpl);
        // execute procedure
        VisualizeDetailsProcCall.SetInputIconicParamObject("Image", Image);
        VisualizeDetailsProcCall.SetInputIconicParamObject("Region", FinRegion);
        VisualizeDetailsProcCall.SetInputCtrlParamTuple("ZoomScale", 2);
        VisualizeDetailsProcCall.SetInputCtrlParamTuple("Margin", 5);
        VisualizeDetailsProcCall.Execute();
        // disable HDevelop's display operators
        MyEngine.SetHDevOperators(null);
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
        return;
      }
      this.Cursor = Cursors.Default;
    }

    private void WindowControl_Load(object sender, EventArgs e)
    {
      Window = WindowControl.HalconWindow;
      // initialize display
      Window.SetDraw("margin");
      Window.SetLineWidth(4);

      // handler for display operators
      MyHDevOperatorImpl = new HDevOpMultiWindowImpl(Window);
    }

  }
}
