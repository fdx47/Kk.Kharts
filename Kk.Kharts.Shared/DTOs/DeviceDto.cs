using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs;

public class DeviceDto
{
 
    public int Id { get; set; }
    [Key()]
    public string DevEui { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InstallationLocation { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public float Battery { get; set; }
    public int? Model { get; set; }
    public string LastSendAt { get; set; } = "??";
    public bool? ActiveInKropKontrol { get; set; }

}


//Serveur FTP: ftp.3ctec.eu
//Port FTP & et FTPS explicite:  21
//Nom d’utilisateur FTP: francois@3ctec.fr
//password: Xeii[C#I==Pv




//Serveur FTP: ftp.3ctec.eu
//Port FTP & et FTPS explicite:  21
//Nom d’utilisateur FTP: francois@kropkontrol.com
//password: Xeii[C#I==Pv

//    www.kropkontrol.com