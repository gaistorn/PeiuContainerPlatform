using Hubbub;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 콘텐츠 대화 상자 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace HubbubMapper.Shared.Dialogs
{
    public sealed partial class SelectSiteDialog : ContentDialog
    {
        const string RestApiServerAddress = "https://www.peiu.co.kr:3020";
        #region ACCESS TOKEN
        const string AccessToken = @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjM2NzQ4N2MzLTZmYzctNGRiZS05NjUzLTYxMzNmMjFkMDZiOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiLqs6DsnYDstZwiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJodHRwczovL3d3dy5wZWl1LmNvLmtyL3dzLzIwMTkvMDkvaWRlbnRpdHkvcm9sZXMvc3VwZXJ2aXNvciIsIlNpdGVJZHNCeVJjYyI6IjE6NzEsMToxMzgsMToyMjc0NDI3MDAsMToxMDIwMTQyNjAzLDE6MTEyNjQ2NTYwNyw0OjE2MCw0OjYyMjI0NzcxOCw0OjEzMTYxMjIwMjAsNDoxMzE2MTIzMzE0LDU6MTA3LDU6MTM3LDU6NjIyMjQwMzg3LDY6Myw2Ojk3LDY6MTIxNjExMzY5Myw2OjEyMTYxMTM3MDAsNjoxMjE2MTEzNzE5LDY6MTIxNjExNTE3NCw3OjMxLDc6MzMsNzoxMjMsNzozMjIxMzE1NTQsNzozMzM4NjMyNzAsOToxLDk6Miw5OjEzNiw5OjcxMDA1MTQ0MCw5OjcxMDA1MzY5OCw5OjcxMDA1NDcyMiw5OjE3MTQ3MjA0OTcsOToxNzE0NzIwNTMwLDEwOjYxLDEwOjE1MCwxMDoxNjEsMTA6MTMxNjExODA4MCwxMDoxMzE2MTIzMzA1LDEwOjEzMTYxMjM0MjEsMTA6MTMyMTk2NTQwNCwxMTo5LDExOjk2LDExOjEwNCwxMToxNDcsMTE6MTQ4LDExOjE0OSwxMTo1MjYzMDcyMjgsMTE6NTI2MzA3MzcxLDEyOjUyNjMxNTcyNywxMzo0MjIyMDMxMTgsMTM6NDIyMjAzMzc4LDEzOjQyMjIwMzYyNiwxMzo0MzEwOTEwMTYsMTQ6NzAsMTU6OTUsMTU6MTE2LDE1OjEyOSwxNTo0MjIyMDY5OTAsMTU6OTE3MTA5OTc5LDE2OjYiLCJleHAiOjE2MzM1OTA3NTgsImlzcyI6Imh0dHBzOi8vd3d3LnBlaXUuY28ua3IiLCJhdWQiOiJjZ2VAcG93ZXIyMS5jby5rciJ9.qX8pj_-cQnvNbCHdFp2_3qyHEL8yx-ljkvypO6qIlK0";
        #endregion




        public Hubbub.ModbusHubbub SelectedHubbub
        {
            get { return (Hubbub.ModbusHubbub)GetValue(SelectedHubbubProperty); }
            set { SetValue(SelectedHubbubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedHubbub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedHubbubProperty =
            DependencyProperty.Register("SelectedHubbub", typeof(Hubbub.ModbusHubbub), typeof(SelectSiteDialog), new PropertyMetadata(null));



        public List<Hubbub.ModbusHubbub> Hubbubs
        {
            get { return (List<Hubbub.ModbusHubbub>)GetValue(HubbubsProperty); }
            set { SetValue(HubbubsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hubbubs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HubbubsProperty =
            DependencyProperty.Register("Hubbubs", typeof(List<Hubbub.ModbusHubbub>), typeof(SelectSiteDialog), new PropertyMetadata(null));



        public SelectSiteDialog()
        {
            this.InitializeComponent();
        }

        private static async Task<ModbusHubbubMappingTemplate> DownloadAnalogTemplateAsync(int hubbubid, string RestApiServerAddress, string AccessToken)
        {
            ModbusHubbubMappingTemplate AnalogTemplates = null;
            try
            {

                var hubbubClient = new RestClient(RestApiServerAddress);
                //hubbubClient.Authenticator = new RestSharp.Authenticators.JwtAuthenticator(AccessToken);
                var request = new RestRequest($"/api/Hubbub/v1/information/{hubbubid}");
                request.Parameters.Add(new Parameter("hubbubid ", hubbubid, ParameterType.GetOrPost));
                request.Parameters.Add(new Parameter("compress", true, ParameterType.GetOrPost));
                var str_result = await hubbubClient.GetAsync<string>(request);

                byte[] datas = Convert.FromBase64String(str_result);

                using (var inputStream = new MemoryStream(datas))
                using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    gZipStream.CopyTo(outputStream);
                    var outputBytes = outputStream.ToArray();

                    string decompressed = System.Text.Encoding.UTF8.GetString(outputBytes);
                    AnalogTemplates = JsonConvert.DeserializeObject<ModbusHubbubMappingTemplate>(decompressed);
                }
            }
            catch (Exception ex)
            {
               
            }
            return AnalogTemplates;
        }

        private async void SelectSiteDialog_Loaded(object sender, RoutedEventArgs e)
        {
            List<Hubbub.ModbusHubbub> AnalogTemplates = null;
            try
            {
                
                var hubbubClient = new RestClient(RestApiServerAddress);
                hubbubClient.Authenticator = new RestSharp.Authenticators.JwtAuthenticator(AccessToken);
                var request = new RestRequest($"/api/Hubbub/v1/information/");
                Hubbubs = await hubbubClient.GetAsync<List<Hubbub.ModbusHubbub>>(request);
                

            }
            catch (Exception ex)
            {
                //if (File.Exists(HubbubTemplateFile))
                //{
                //    AnalogTemplates = JsonConvert.DeserializeObject<ModbusHubbubMappingTemplate>(File.ReadAllText(templateOutputFile, System.Text.Encoding.UTF8));
                //    logger.Warn(ex, $"템플릿 정보를 가져오는 것에 실패했습니다. 기존의 템플릿 정보를 반영합니다");
                //}
                //else
                //{
                //    logger.Error(ex, $"템플릿 정보를 가져오는 것에 실패했습니다. 다음 {RestApiServerAddress} 서버의 접속 오류\n{ex.Message}");
                //    throw;
                //    //throw new Exception($"템플릿 정보를 가져오는 것에 실패했습니다. 다음 {RestApiServerAddress} 서버의 접속 오류");
                //}
            }
        }

        

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
            if(SelectedHubbub == null)
            {
                args.Cancel = true;
                //NotSelectTip.IsOpen = true;
                MessageDialog dlg = new MessageDialog("선택된 사이트가 없습니다");
                await dlg.ShowAsync();
                return;
            }

            GlobalProperty.Common.HubbubTemplate = await DownloadAnalogTemplateAsync(SelectedHubbub.Id, RestApiServerAddress, AccessKey);
            GlobalProperty.Common.Hubbub = GlobalProperty.Common.HubbubTemplate.Hubbub;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedHubbub != null)
            {
                this.DefaultButton = ContentDialogButton.Primary;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
        }
    }
}
