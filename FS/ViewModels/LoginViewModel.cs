using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private List<DefaultServer> defaultPublicServers;

    [ObservableProperty]
    private string loginState;
    public LoginViewModel()
    {
        defaultPublicServers = new List<DefaultServer>
        {
            new DefaultServer("AARNet", new Uri("https://filesender.aarnet.edu.au/")), 
            new DefaultServer("ACOnet", new Uri("https://filesender.aco.net/")),
            new DefaultServer("AMRES", new Uri("https://filesender.amres.ac.rs/")),
            new DefaultServer("APAN", new Uri("https://apacfilesender.asia/")),
            new DefaultServer("ARNES", new Uri("https://filesender.arnes.si/")),
            new DefaultServer("AzScienceNet", new Uri("https://filesender.azsciencenet.az")),
            new DefaultServer("BdREN", new Uri("https://filesender.bdren.net.bd/")),
            new DefaultServer("BELNET", new Uri("https://filesender.belnet.be/")),
            new DefaultServer("CEDIA", new Uri("https://filesender.cedia.org.ec")),
            new DefaultServer("CESNET", new Uri("https://filesender.cesnet.cz")),
            new DefaultServer("CUDI", new Uri("https://cudi.edu.mx/servicios/envio")),
            new DefaultServer("DeiC", new Uri("https://filesender.deic.dk")),
            new DefaultServer("EthERNet", new Uri("https://filesender.ethernet.edu.et/")),
            new DefaultServer("FCCN", new Uri("https://filesender.fccn.pt")),
            new DefaultServer("Frederick University", new Uri("https://transfer.frederick.ac.cy")),
            new DefaultServer("FUNET/CSC", new Uri("https://filesender.funet.fi/")),
            new DefaultServer("GARR", new Uri("https://filesender.garr.it/")),
            new DefaultServer("GÉANT", new Uri("https://filesender.geant.org/")),
            new DefaultServer("Hebrew University of Jerusalem (HUJI)", new Uri("https://filesender.huji.ac.il")),
            new DefaultServer("Helmholtz Federated IT Services (HIFIS)/German Cancer Research Center (DKFZ)", new Uri("https://filesender.hifis.dkfz.de/")),
            new DefaultServer("Institute for Advanced Studies", new Uri("https://filesender.ihs.ac.at")),
            new DefaultServer("IPM/IRANET", new Uri("https://filesender.ipm.ir")),
            new DefaultServer("KENET", new Uri("https://filesender.kenet.or.ke/")),
            new DefaultServer("KIFÜ", new Uri("https://filesender.hu")),
            new DefaultServer("MARWAN", new Uri("https://filesender.marwan.ma/filesender")),
            new DefaultServer("MYREN", new Uri("https://filesender.myren.net.my")),
            new DefaultServer("NII/GakuNin", new Uri("https://filesender.nii.ac.jp/")),
            new DefaultServer("Okinawa Institute of Science and Technology", new Uri("https://filesender.oist.jp")),
            new DefaultServer("OMREN", new Uri("https://mirsal.omren.om/")),
            new DefaultServer("Pleio - Dutch government", new Uri("https://bestandendelen.pleio.nl/filesender")),
            new DefaultServer("PSNC/PIONIER", new Uri("https://files.pionier.net.pl/")),
            new DefaultServer("RASH", new Uri("https://filesender.rash.al/")),
            new DefaultServer("REANNZ", new Uri("https://filesender.reannz.co.nz")),
            new DefaultServer("RedCLARA", new Uri("https://filesender.redclara.net/filesender/")),
            new DefaultServer("RedIRIS", new Uri("https://filesender.rediris.es")),
            new DefaultServer("RENAM", new Uri("https://filesender.renam.md")),
            new DefaultServer("RENATER", new Uri("https://filesender.renater.fr/")),
            new DefaultServer("RENU", new Uri("https://filesender.renu.ac.ug/")),
            new DefaultServer("RESTENA", new Uri("https://fs.restena.lu")),
            new DefaultServer("REUNA", new Uri("https://filesender.reuna.cl")),
            new DefaultServer("RNP", new Uri("https://filesender.rnp.br")),
            new DefaultServer("RoEduNet", new Uri("https://fisiere.roedu.net/")),
            new DefaultServer("RUNNet", new Uri("https://filesender.runnet.ru/")),
            new DefaultServer("SANReN", new Uri("https://filesender.sanren.ac.za/filesender/")),
            new DefaultServer("Sikt", new Uri("https://filesender.sikt.no/")),
            new DefaultServer("SingAREN", new Uri("https://filesender.singaren.net.sg/filesender/")),
            new DefaultServer("SomaliREN", new Uri("https://filesender.somaliren.org.so/filesender/")),
            new DefaultServer("SRCE", new Uri("https://filesender.srce.hr/")),
            new DefaultServer("SURF", new Uri("https://filesender.surf.nl/")),
            new DefaultServer("Switch", new Uri("https://filesender.switch.ch")),
            new DefaultServer("UBUNTUNET", new Uri("https://filesender.ubuntunet.net/")),
            new DefaultServer("ULAKBIM", new Uri("https://filesender.ulakbim.gov.tr")),
            new DefaultServer("University of Cyprus", new Uri("https://filesender.ucy.ac.cy")),
            new DefaultServer("University of West Attica", new Uri("https://filesender.uniwa.gr/")),
            new DefaultServer("URAN", new Uri("https://filesender.uran.ua/filesender/")) 
        };
            loginState = "Select Your Server";

    }

}