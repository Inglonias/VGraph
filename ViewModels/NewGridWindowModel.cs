using System;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VGraph.Config;
using VGraph.DataLayers;

namespace VGraph.ViewModels;

public partial class NewGridWindowModel : ViewModelBase
{
    public required bool DeleteLines { get; set; }
    [ObservableProperty] public partial string WindowTitle { get; set; }
    [ObservableProperty] public partial string? ImageInfoTextBlock { get; private set; }
    [ObservableProperty] public partial string ImagePath { get; set; }
    [ObservableProperty] public partial string GridSquaresWide { get; set; }
    [ObservableProperty] public partial string GridSquaresTall { get; set; }
    [ObservableProperty] public partial string GridSquareSize { get; set; }
    [ObservableProperty] public partial string PageMarginX { get; set; }
    [ObservableProperty] public partial string PageMarginY { get; set; }

    [ObservableProperty] public partial int BackgroundImageOpacitySliderValue { get; set; }
    
    public ICommand PressOkCommand { get; }

    partial void OnGridSquaresWideChanged(string value) => CalculateImageInfo();
    partial void OnGridSquaresTallChanged(string value) => CalculateImageInfo();
    partial void OnGridSquareSizeChanged(string value) => CalculateImageInfo();
    partial void OnPageMarginXChanged(string value) => CalculateImageInfo();
    partial void OnPageMarginYChanged(string value) => CalculateImageInfo();
    partial void OnImagePathChanged(string value) => CalculateImageInfo();

    public NewGridWindowModel()
    {
        PressOkCommand = new RelayCommand(FinishGridSetup);
        
        WindowTitle = DeleteLines ? "Create New Grid" : "Resize Grid";
        GridSquaresWide = Convert.ToString(PageData.Instance.SquaresWide);
        GridSquaresTall = Convert.ToString(PageData.Instance.SquaresTall);
        GridSquareSize = Convert.ToString(PageData.Instance.TrueSquareSize);
        PageMarginX = Convert.ToString(PageData.Instance.MarginX);
        PageMarginY = Convert.ToString(PageData.Instance.MarginY);
        BackgroundImageOpacitySliderValue = PageData.Instance.BackgroundImageAlpha;
        ImagePath = PageData.Instance.BackgroundImagePath;
        CalculateImageInfo();
    }

    public void FinishGridSetup()
    {
        try
        {
            PageData.Instance.SquaresWide = Math.Max(1, Convert.ToInt32(GridSquaresWide));
            PageData.Instance.SquaresTall = Math.Max(1, Convert.ToInt32(GridSquaresTall));
            PageData.Instance.SquareSize = Math.Min(128, Math.Max(4, Convert.ToInt32(GridSquareSize)));
            PageData.Instance.TrueSquareSize = Math.Min(128, Math.Max(4, Convert.ToInt32(GridSquareSize)));
            PageData.Instance.MarginX = Math.Max(0, Convert.ToInt32(PageMarginX));
            PageData.Instance.MarginY = Math.Max(0, Convert.ToInt32(PageMarginY));
            string path = ImagePath;
            PageData.Instance.SetBackgroundImage(path);
            PageData.Instance.BackgroundImageAlpha = Convert.ToByte(BackgroundImageOpacitySliderValue);
            GridBackgroundLayer gridBackgroundLayer =
                (GridBackgroundLayer)PageData.Instance.GetDataLayers()[PageData.GRID_LAYER];
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayers()[PageData.LINE_LAYER];
            TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayers()[PageData.TEXT_LAYER];

            gridBackgroundLayer.ForceRedraw();
            if (DeleteLines)
            {
                PageData.Instance.LastSavePath = String.Empty;
                PageData.Instance.MakeCanvasClean();
                lineLayer.ClearAllLines();
                textLayer.ClearAllLabels();
            }

            lineLayer.ForceRedraw();
        }
        catch (FormatException)
        {
        }
    }
    
    private void CalculateImageInfo()
    {
        const string templateText =
            "Canvas size ----- (px) : [CANVSIZE]\nGrid size ------- (px) : [GRIDSIZE]\nBackground size - (px) : [BACKSIZE]";

        bool anyInfoValid = false;
        ImageInfoTextBlock = "Please enter valid info for image size data.";
        string displayText = templateText;
        try
        {
            int squaresWide = Convert.ToInt32(GridSquaresWide);
            int squaresTall = Convert.ToInt32(GridSquaresTall);
            int squareSize = Convert.ToInt32(GridSquareSize);
            int marginX = Convert.ToInt32(PageMarginX);
            int marginY = Convert.ToInt32(PageMarginY);

            anyInfoValid = true;

            int gridWidth = squaresWide * squareSize;
            int gridHeight = squaresTall * squareSize;
            int canvasWidth = gridWidth + (marginX * 2);
            int canvasHeight = gridHeight + (marginY * 2);

            string canvSizeString = canvasWidth.ToString().PadLeft(6) + " x " + canvasHeight.ToString().PadRight(6);
            string gridSizeString = gridWidth.ToString().PadLeft(6) + " x " + gridHeight.ToString().PadRight(6);
            displayText = displayText.Replace("[CANVSIZE]", canvSizeString);
            displayText = displayText.Replace("[GRIDSIZE]", gridSizeString);
        }
        catch (FormatException)
        {
            displayText = displayText.Replace("[CANVSIZE]", "   N/A x N/A   ");
            displayText = displayText.Replace("[GRIDSIZE]", "   N/A x N/A   ");
        }

        if (File.Exists(ImagePath))
        {
            anyInfoValid = true;
            SKImageInfo backgroundInfo = SKBitmap.Decode(new SKFileStream(ImagePath)).Info;

            int backWidth = backgroundInfo.Width;
            int backHeight = backgroundInfo.Height;

            string backSizeString = backWidth.ToString().PadLeft(6) + " x " + backHeight.ToString().PadRight(6);
            displayText = displayText.Replace("[BACKSIZE]", backSizeString);
        }
        else
        {
            displayText = displayText.Replace("[BACKSIZE]", "   N/A x N/A   ");
        }

        if (anyInfoValid)
        {
            ImageInfoTextBlock = displayText;
        }
    }
}