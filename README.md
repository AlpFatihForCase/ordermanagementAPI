Bu proje soruda da belirtildiği gibi sadece endpoint kısımlarını içerir. DB ye bağlantısı yoktur ve basit bellek içi bir simülasyon içerir. Talep edildiği halde db bağlantısı yapabilir hatta istenirse dockerize edip de postman env i oluşturulup bir arada da payalaşabilirim. Mail i son gün gördüğüm için bu kısımları yapamadım(istenmemişti zaten) ama istenirse rahatlıkla ve hızla bu kısımları da yapabilir ve herhangi bir ortama deploy edebilirim. 




Token Sorusu 

Bu senaryo için bir .NET Core Worker Service uygulaması yazdım adı OrderListFetcher. İlk olarak API bilgilerini ve çalışma sıklığını ayarladım. Sonra token alıp yöneten bir servis oluşturdum. Bu servis token'ı alıyor, ne kadar geçerli olduğunu biliyor ve saatte en fazla 5 kere token isteme kuralına uyuyor. Sipariş listesini almak için ayrı bir servis yazdım, bu servis de token servisinden geçerli token'ı alıp API'ye soruyor. Tüm bu işleri düzenli olarak yapması için de bir arka plan çalışanı ayarladım. Son olarak da bu parçaların hepsini Program.cs ile birbirine bağladım.