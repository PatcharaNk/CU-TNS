// HDevEngine/.NET (C#) example for executing external 
// HDevelop procedures
//
// © 2007-2017 MVTec Software GmbH
//
// Purpose:
// This example program shows how the classes HDevEngine, HDevProcedureCall,
// and HDevOpMultiWindowImpl can be used in order to implement a fin detection
// application. Most of the application's functionality is contained in the
// HDevelop external procedure detect_fin(), which can be found in the
// procedures directory. The procedure takes an image as input object parameter
// and returns a region and its area as output parameters.
// When you click the button Load, the HDevelop procedure is loaded;
// when you click Execute, the following steps are executed:
// 1. An image is grabbed and passed as input parameter to the procedure call.
// 2. The procedure call is executed.
// 3. The output parameters of the procedure call are retrieved and
//    displayed in the graphics window.
// These steps are executed repeatedly for different images. The class
// HDevOpMultiWindowImpl implements HDevelop's internal operators.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using HalconDotNet;

namespace ExecExtProc
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class ExecExtProcForm : System.Windows.Forms.Form
  {
    internal System.Windows.Forms.Button LoadBtn;
    internal System.Windows.Forms.Button ExecuteBtn;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    // HDevEngine
    // instance of the engine
    private HDevEngine MyEngine = new HDevEngine();
    // implementation of the display operators
    // private HDevOpMultiWindowImpl MyHDevOperatorImpl;

    // procedure call
    private HDevProcedureCall ProcCall;
    private HSmartWindowControl WindowControl;
    // HALCON window
    private HWindow Window;

    public ExecExtProcForm()
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
      this.ExecuteBtn = new System.Windows.Forms.Button();
      this.WindowControl = new HalconDotNet.HSmartWindowControl();
      this.SuspendLayout();
      // 
      // LoadBtn
      // 
      this.LoadBtn.Location = new System.Drawing.Point(424, 8);
      this.LoadBtn.Name = "LoadBtn";
      this.LoadBtn.Size = new System.Drawing.Size(120, 40);
      this.LoadBtn.TabIndex = 3;
      this.LoadBtn.Text = "Load Procedure";
      this.LoadBtn.Click += new System.EventHandler(this.LoadBtn_Click);
      // 
      // ExecuteBtn
      // 
      this.ExecuteBtn.Location = new System.Drawing.Point(424, 64);
      this.ExecuteBtn.Name = "ExecuteBtn";
      this.ExecuteBtn.Size = new System.Drawing.Size(120, 40);
      this.ExecuteBtn.TabIndex = 6;
      this.ExecuteBtn.Text = "Execute Procedure";
      this.ExecuteBtn.Click += new System.EventHandler(this.ExecuteBtn_Click);
      // 
      // WindowControl
      // 
      this.WindowControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.WindowControl.HImagePart = new System.Drawing.Rectangle(0, 0, 768, 576);
      this.WindowControl.Location = new System.Drawing.Point(16, 8);
      this.WindowControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.WindowControl.Name = "WindowControl";
      this.WindowControl.Size = new System.Drawing.Size(384, 288);
      this.WindowControl.TabIndex = 7;
      this.WindowControl.WindowSize = new System.Drawing.Size(384, 288);
      this.WindowControl.Load += new System.EventHandler(this.WindowControl_Load);
      // 
      // ExecExtProcForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(560, 309);
      this.Controls.Add(this.WindowControl);
      this.Controls.Add(this.ExecuteBtn);
      this.Controls.Add(this.LoadBtn);
      this.Name = "ExecExtProcForm";
      this.Text = "Execute External HDevelop Procedures via HDevEngine";
      this.Load += new System.EventHandler(this.ExecExtProcForm_Load);
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
      Application.Run(new ExecExtProcForm());
    }

    private void ExecExtProcForm_Load(object sender, System.EventArgs e)
    {
      // path of external procedures
      string halconExamples = HSystem.GetSystem("example_dir");
      string ProcedurePath = halconExamples + @"\hdevengine\procedures";

      if (!HalconAPI.isWindows)
      {
      // Unix-based systems (Mono)
      ProcedurePath = ProcedurePath.Replace('\\', '/');
      }
      MyEngine.SetProcedurePath(ProcedurePath);

      // disable Execute button
      ExecuteBtn.Enabled = false;
    }

    private void LoadBtn_Click(object sender, System.EventArgs e)
    {
      try
      {
        HDevProcedure Procedure = new HDevProcedure("detect_fin");
        ProcCall = new HDevProcedureCall(Procedure);
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
        return;
      }

      // enable Execute button
      LoadBtn.Enabled = false;
      ExecuteBtn.Enabled = true;
    }


    private void ExecuteBtn_Click(object sender, System.EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;

      HFramegrabber Framegrabber = new HFramegrabber();

      // read images and process them
      try
      {
        Framegrabber.OpenFramegrabber("File", 1, 1, 0, 0, 0, 0, "default",
           -1, "default", -1, "default", "fin.seq", "default", -1, -1);

        HImage Image = new HImage();
        HRegion FinRegion;
        HTuple FinArea;

        for (int i=0; i<=2; i++)
        {
          Image.GrabImage(Framegrabber);
          Image.DispObj(Window);

          // execute procedure
          ProcCall.SetInputIconicParamObject("Image", Image);
          ProcCall.Execute();
          // get output parameters from procedure call
          FinRegion = ProcCall.GetOutputIconicParamRegion("FinRegion");
          FinArea = ProcCall.GetOutputCtrlParamTuple("FinArea");

          // display results
          Image.DispObj(Window);
          Window.SetColor("red");
          Window.DispObj(FinRegion);
          Window.SetColor("white");
          Window.SetTposition(150, 20);
          Window.WriteString("FinArea: " + FinArea.D);
          HSystem.WaitSeconds(2);

          FinRegion.Dispose();
          Image.Dispose();
        }
      }
      catch (HOperatorException Ex)
      {
        MessageBox.Show(Ex.Message, "HALCON Exception");
      }
      catch (HDevEngineException Ex)
      {
        MessageBox.Show(Ex.Message, "HDevEngine Exception");
      }

      Framegrabber.Dispose();

      this.Cursor = Cursors.Default;
    }

    private void WindowControl_Load(object sender, EventArgs e)
    {
      Window = WindowControl.HalconWindow;
      // initialize display
      Window.SetDraw("margin");
      Window.SetLineWidth(4);

      // handler for display operators
      // to use handler, uncomment the following lines
      // MyHDevOperatorImpl = new HDevOpMultiWindowImpl(Window);
      // MyEngine.SetHDevOperators(MyHDevOperatorImpl);
    }

  }
}
