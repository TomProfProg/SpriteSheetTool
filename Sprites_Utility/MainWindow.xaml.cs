using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Sprites_Utility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        BitmapImage imageToSplit;

        public MainWindow()
        {
            InitializeComponent();
            UpdateButtons();
        }


        void UpdateButtons()
        {
            SaveAsButtonSplitter.IsEnabled = ImageBoxSplitter.Source != null;
            SaveAsButtonCombiner.IsEnabled = ImageBoxCombiner.Source != null;

        }
        private void OpenButtonSplitter_Click(object sender, RoutedEventArgs e)
        {
            string fileName;
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PNG images|*.png|Bitmap Images|*.bmp|JPEG Images|*.jpg;jpeg|All formats|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;

                Uri imageUri = new Uri(fileName);
                imageToSplit = new BitmapImage(imageUri);
                ImageBoxSplitter.Source = imageToSplit;

            }



        }

        bool ApplyParameterSplitter(string _fileName, string extension)
        {
            if (imageToSplit != null)
            {
                int width = imageToSplit.PixelWidth;
                int height = imageToSplit.PixelHeight;

                int columnNbr;
                int.TryParse(columnSplitter.Text, out columnNbr);

                int rowNbr;
                int.TryParse(rowSplitter.Text, out rowNbr);

                if(columnNbr == 0 || rowNbr ==0)
                    return false;
                int rectWidth = width / columnNbr;
                int rectHeight = height / rowNbr;

                for (int y = 0; y < rowNbr; y++)
                {
                    for (int x = 0; x < columnNbr; x++)
                    {
                        //imageToSplit.SourceRect = new Int32Rect(x * rectWidth, y * rectHeight, rectWidth, rectHeight);
                        BitmapSource source = new CroppedBitmap(imageToSplit, new Int32Rect(x * rectWidth, y * rectHeight, rectWidth, rectHeight));
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(source));
                        using (var fileStream = new System.IO.FileStream(_fileName + "_" + y.ToString() + "_" + x.ToString() + extension, System.IO.FileMode.Create))
                        {
                            encoder.Save(fileStream);
                            
                        }
                    }
                }
                return true;
            }
            return false;
        }


        void ApplyParameterCombiner(string _fileName, string extension)
        {
            if (wBitmap != null)
            {
                //imageToSplit.SourceRect = new Int32Rect(x * rectWidth, y * rectHeight, rectWidth, rectHeight);
                BitmapSource source = wBitmap;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                using (var fileStream = new System.IO.FileStream(_fileName + extension, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        private void SaveAsButtonSplitter_Click(object sender, RoutedEventArgs e)
        {

            if (ImageBoxSplitter.Source != null)
            {
                saveFileDialog.Filter = "PNG images|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    ApplyParameterSplitter(saveFileDialog.FileName, ".png");
                }
            }
            else
            {
                MessageBox.Show("Impossible d'exporter les multiples sprites: Aucune image originale n'a été chargée.");
            }
        }

        WriteableBitmap wBitmap;
        private void OpenButtonCombiner_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "PNG images|*.png|Bitmap Images|*.bmp|JPEG Images|*.jpg;jpeg|All formats|*.*";

            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string[] fileNames = openFileDialog.FileNames;

                BitmapImage[] bitmapImageCombinedSource = new BitmapImage[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    Uri uri = new Uri(fileNames[i]);
                    bitmapImageCombinedSource[i] = new BitmapImage(uri);
                }

                int nbImages = bitmapImageCombinedSource.Length;
                int nbColumns = (int)MathF.Ceiling(MathF.Sqrt(nbImages));
                int overRow = (int)(((nbColumns * nbColumns) - nbImages) / nbColumns);
                int nbRows = nbColumns - overRow;
                int baseWidth = bitmapImageCombinedSource[0].PixelWidth;
                int baseHeight = bitmapImageCombinedSource[0].PixelHeight;

                // Draw a Rectangle
                DrawingVisual dVisual = new DrawingVisual();
                if (bitmapImageCombinedSource.Length >= 1)
                {
                    using (DrawingContext dc = dVisual.RenderOpen())
                    {
                        for (int y = 0; y < nbRows; y++)
                        {
                            for (int x = 0; x < nbColumns; x++)
                            {
                                int index = y * nbColumns + x;
                                if (index >= nbImages) break;
                                //Draw à la position dans le rectangle destination
                                dc.DrawImage(bitmapImageCombinedSource[index], new Rect(x * baseWidth, y * baseHeight, bitmapImageCombinedSource[index].PixelWidth, bitmapImageCombinedSource[index].PixelHeight));
                            }
                        }
                    }
                }
                //taille de la texture final à définir dans le constructeur ici
                RenderTargetBitmap targetBitmap = new RenderTargetBitmap(baseWidth * nbColumns, baseHeight * nbRows, 96, 96, PixelFormats.Default);
                targetBitmap.Render(dVisual);


                //on affiche l'image finale
                wBitmap = new WriteableBitmap(targetBitmap);
                ImageBoxCombiner.Source = wBitmap;
            }
        }

        private void SaveAsButtonCombiner_Click(object sender, RoutedEventArgs e)
        {
            if (ImageBoxCombiner.Source != null)
            {
                saveFileDialog.Filter = "PNG images|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    ApplyParameterCombiner(saveFileDialog.FileName, ".png");

                }
            }
            else
            {
                MessageBox.Show("Impossible d'exporter l'image combinée': Aucune image originale n'a été chargée.");
            }
        }

        private void FileButtonCombiner_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtons();
        }

        private void FileButtonSplitter_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtons();
        }
    }
}
