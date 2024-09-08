using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using mexLib.Types;
using System;
using System.Linq;
using System.Reactive.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MexManager.Controls;

public partial class CodeBox : UserControl
{
    private readonly static IBrush CompileSuccess = Brushes.Green;

    private readonly static IBrush CompileFailed = Brushes.Red;

    /// <summary>
    /// 
    /// </summary>
    public CodeBox()
    {
        InitializeComponent();

        UpdateLineNumbers();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    public void SetError(string? error)
    {
        ErrorBlock.Text = error;

        if (string.IsNullOrEmpty(error))
        {
            ErrorBlock.Foreground = CompileSuccess;
        }
        else
        {
            ErrorBlock.Foreground = CompileFailed;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLineNumbers();

        SetError(null);
        if (DataContext is MexCode code)
        {
            if (code.CompileError != null)
            {
                SetError(code.CompileError.ToString());
            }
            else if (Global.Workspace != null)
            {
                foreach (var c in Global.Workspace.Project.GetAllCodes())
                {
                    if (c == code)
                        continue;

                    var res = code.TryCheckConflicts(c);
                    if (res != null)
                    {
                        SetError(res.ToString());
                        break;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void UpdateLineNumbers()
    {
        if (MainTextBox.Text == null)
            return;

        var lines = MainTextBox.Text.Split('\n');
        LineNumbers.Text = string.Join("\n", Enumerable.Range(0, lines.Length));
    }
}