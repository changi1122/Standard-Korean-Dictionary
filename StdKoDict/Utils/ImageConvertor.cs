using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace 표준국어대사전.Utils
{
    internal class ImageConvertor
    {
        /// <summary>
        /// Base64 인코딩 이미지 문자열을 BitmapImage로 변환합니다.
        /// </summary>
        /// <param name="source">Base64 인코딩 이미지 문자열</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage Base64StringToBitmap(string source)
        {
            var ims = new InMemoryRandomAccessStream();
            var bytes = Convert.FromBase64String(source);
            var dataWriter = new DataWriter(ims);
            dataWriter.WriteBytes(bytes);
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            dataWriter.StoreAsync();
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            ims.Seek(0);
            var img = new BitmapImage();
            img.SetSource(ims);
            return img;
        }
    }
}
