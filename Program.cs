// See https://aka.ms/new-console-template for more information
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;

Console.WriteLine("Hello, World!");

// string csv = "";

// using (var driver = UndetectedChromeDriver.Create(
//     driverExecutablePath: @"/home/nulzhar/Público/chromedriver_linux64/chromedriver"))
// {
//     driver.GoToUrl("https://processo.stj.jus.br/processo/pesquisa/?aplicacao=processos.ea");
//     Thread.Sleep(1000);

//     // ##############################
//     // # BUSCA PELA PARTE
//     // ##############################

//     var element = driver.FindElement(By.Id("idInterfaceVisualAreaBlocoInterno"));
//     if (element.Text.ToLower().Contains("sistema indisponível"))
//     {
//         Console.WriteLine("SISTEMA INDISPONIVEL");
//     }

//     element = driver.FindElement(By.Id("idParteNome"));
//     element.SendKeys("Banco Itau");

//     element = driver.FindElement(By.Id("idBotaoPesquisarFormularioExtendido"));
//     element.Click();

//     // ##############################
//     // # Seleciona todas as ref de parte
//     // ##############################

//     element = driver.FindElement(By.Id("idBotaoMarcarTodos"));
//     element.Click();
//     try
//     {
//         element = driver.FindElement(By.Id("idBotaoPesquisarMarcados"));
//         element.Click();
//     }
//     catch (System.Exception)
//     {
//     }

//     // ##############################
//     // # AGUARDA O BLOCO DE MENSAGEM
//     // ##############################

//     try
//     {
//         element = new WebDriverWait(driver, TimeSpan.FromSeconds(600))
//         .Until(drv => drv.FindElement(By.Id("idDivBlocoMensagem")));
//     }
//     catch (Exception ex)
//     {

//     }

//     var scrollPauseTime = 20;
//     var lastHeight = driver.ExecuteScript("return document.body.scrollHeight");
//     while(true)
//     {
//         driver.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
//         Thread.Sleep(scrollPauseTime);

//         var newHeight = driver.ExecuteScript("return document.body.scrollHeight");

//         if ((long)newHeight == (long)lastHeight)
//         {
//             break;
//         }

//         lastHeight = newHeight;
//     }


//     element = driver.FindElement(By.Id("idBotaoCopiarColarCsv"));
//     element.Click();
    
//     IAlert alert = driver.SwitchTo().Alert();
//     alert.Accept();
    
//     try
//     {
//         element = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
//         .Until(drv => drv.FindElement(By.Id("idCopiarParaCSV")));
//         csv = element.Text;
//     }
//     catch (Exception ex)
//     {

//     }
    
//     using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"/home/nulzhar/Público/", "WriteLines.txt")))
//     {
//         outputFile.WriteLine(csv);
//     }
// }

// // OBTER DETALHE PROCESSO

string csv = "";

try
{
    // Create an instance of StreamReader to read from a file.
    // The using statement also closes the StreamReader.
    using (StreamReader sr = new StreamReader(Path.Combine(@"/home/nulzhar/Público/", "WriteLines.txt")))
    {
        string line;
        // Read and display lines from the file until the end of
        // the file is reached.
        while ((line = sr.ReadLine()) != null)
        {
            csv += line + "\n";
        }
    }
}
catch (Exception e)
{
    // Let the user know what went wrong.
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message);
}

var listaProcessos = csv.Split("\n");

// var campos = "AREsp,,RS,201701925806,50045948320228216001,,,,ROBERTO LUIZ ALVES DOS SANTOS (AGRAVADO) |BANCO ITAU CONSIGNADO S.A. (AGRAVANTE) |,TRIBUNAL DE JUSTIÇA DO ESTADO DO RIO GRANDE DO SUL,50045948320228216001.,eletrônico,DIREITO DO CONSUMIDOR|Cláusulas Abusivas.,DIREITO CIVIL,06/09/2023,Recebidos os autos eletronicamente no(a) SUPERIOR TRIBUNAL DE JUSTIÇA do TRIBUNAL DE JUSTIÇA DO ESTADO DO RIO GRANDE DO SUL,".Split(",");

int count = 0;
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
foreach (var item in listaProcessos)
{
    var campos = item.Split(",");

    if (campos.Length < 16)
        continue;

    if (campos[3] == "Registro")
        continue;

    if (count == 100)
        break;

    Extrair(campos);
    count++;
    Thread.Sleep(3000);
}
stopwatch.Stop();

var lote = new
{
    Quantidade = count,
    Tempo = stopwatch.ElapsedMilliseconds,
};

string output = JsonConvert.SerializeObject(lote);

using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"/home/nulzhar/Público/", "lote.txt"), true))
{
    outputFile.WriteLine(output);
    outputFile.WriteLine();
}

static void Extrair(string[] campos)
{
    using (var driver = UndetectedChromeDriver.Create(
        driverExecutablePath: @"/home/nulzhar/Público/chromedriver_linux64/chromedriver"))
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        driver.GoToUrl("https://processo.stj.jus.br/processo/pesquisa/?aplicacao=processos.ea");
        Thread.Sleep(1000);

        // ##############################
        // # BUSCA PELA PROCESSO
        // ##############################

        var element = driver.FindElement(By.Id("idInterfaceVisualAreaBlocoInterno"));
        if (element.Text.ToLower().Contains("sistema indisponível"))
        {
            Console.WriteLine("SISTEMA INDISPONIVEL");
            return;
        }

        element = driver.FindElement(By.Id("idNumeroRegistro"));
        element.SendKeys(campos[3]);

        element = driver.FindElement(By.Id("idBotaoPesquisarFormularioExtendido"));
        element.Click();

        // AGUARDA O CAMPO QUE MARCA QUE ESTA NO DETALHE DO PROCESSO
        try
        {
            element = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(drv => drv.FindElement(By.Id("idDescricaoProcesso")));
        }
        catch (Exception ex)
        {

        }

        // 

        try
        {
            element = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(drv => drv.FindElement(By.XPath("//*[@id=\"cf-error-details\"]/header/h2")));

            Console.WriteLine("RATE LIMIT");
        }
        catch (Exception ex)
        {
            
        }

        // DETALHES
        var linhaDetalhes = driver.FindElements(By.ClassName("classDivLinhaDetalhes"));

        var detalhes = new List<Detalhe>();

        foreach (var item in linhaDetalhes)
        {
            var label = item.FindElement(By.ClassName("classSpanDetalhesLabel"));
            var texto = item.FindElement(By.ClassName("classSpanDetalhesTexto"));

            detalhes.Add(new Detalhe(label.Text, texto.Text));
        }

        // FASES

        element = driver.FindElement(By.Id("idSpanAbaFases"));
        element.Click();

        IEnumerable<IWebElement> linhaFases = new List<IWebElement>();
        try
        {
            linhaFases = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(drv => drv.FindElements(By.ClassName("classDivFaseLinha")));
        }
        catch (Exception ex)
        {

        }

        var fases = new List<Fase>();

        foreach (var item in linhaFases)
        {
            var data = item.FindElement(By.ClassName("classSpanFaseData"));
            var hora = item.FindElement(By.ClassName("classSpanFaseHora"));
            var texto = item.FindElement(By.ClassName("classSpanFaseTexto"));

            fases.Add(new Fase(data.Text, hora.Text, texto.Text));
        }


        // Peticao

        element = driver.FindElement(By.Id("idSpanAbaPeticoes"));
        element.Click();

        IEnumerable<IWebElement> linhaPeticoes = new List<IWebElement>();
        try
        {
            linhaPeticoes = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(drv => drv.FindElements(By.ClassName("classDivLinhaPeticoes")));
        }
        catch (Exception ex)
        {

        }

        var peticoes = new List<Peticao>();

        foreach (var item in linhaPeticoes)
        {
            //string numeroProtocolo, string tipo, string dataProtocolo, string dataProcessamento, string requisitante
            var numeroProtocolo = item.FindElement(By.ClassName("classSpanLinhaPeticoesNum"));
            var tipo = item.FindElement(By.ClassName("classSpanLinhaPeticoesTipo"));
            var dataProtocolo = item.FindElement(By.ClassName("classSpanLinhaPeticoesProtocolo"));
            var dataProcessamento = item.FindElement(By.ClassName("classSpanLinhaPeticoesProcessamento"));
            var requisitante = item.FindElement(By.ClassName("classSpanLinhaPeticoesQuem"));

            peticoes.Add(new Peticao(numeroProtocolo.Text, tipo.Text, dataProtocolo.Text, dataProcessamento.Text, requisitante.Text));
        }

        // Pautas

        element = driver.FindElement(By.Id("idSpanAbaPautas"));
        element.Click();

        IEnumerable<IWebElement> linhaPautas = new List<IWebElement>();
        try
        {
            linhaPautas = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(drv => drv.FindElements(By.ClassName("clsDivLinhaPautas")));
        }
        catch (Exception ex)
        {

        }

        var pautas = new List<Pauta>();

        foreach (var item in linhaPautas)
        {
            var data = item.FindElement(By.ClassName("clsLinhaPautasDataJulgamento"));
            var hora = item.FindElement(By.ClassName("clsLinhaPautasHoraJulgamento"));
            var orgaoJulgamento = item.FindElement(By.ClassName("clsLinhaPautasOrgaoJulgamento"));

            pautas.Add(new Pauta(data.Text, hora.Text, orgaoJulgamento.Text));
        }

        stopwatch.Stop();

        var consulta = new
        {
            RegistroPesquisa = campos[3],
            Detalhes = detalhes,
            Fases = fases,
            Peticoes = peticoes,
            Pautas = pautas,
            TempoCrawler = stopwatch.ElapsedMilliseconds,
        };

        string output = JsonConvert.SerializeObject(consulta);

        using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"/home/nulzhar/Público/", "processamento.txt"), true))
        {
            outputFile.WriteLine(output);
        }
    }
}

public class Detalhe
{
    public Detalhe(string tipo, string valor)
    {
        Tipo = tipo;
        Texto = valor;
    }

    public string Tipo { get; set; }
    public string Texto { get; set; }
}

public class Fase
{
    public Fase(string data, string hora, string texto)
    {
        Data = data;
        Hora = hora;
        Texto = texto;
    }

    public string Data { get; set; }
    public string Hora { get; set; }
    public string Texto { get; set; }
}

public class Peticao
{
    public Peticao(string numeroProtocolo, string tipo, string dataProtocolo, string dataProcessamento, string requisitante)
    {
        NumeroProtocolo = numeroProtocolo;
        Tipo = tipo;
        DataProtocolo = dataProtocolo;
        DataProcessamento = dataProcessamento;
        Requisitante = requisitante;
    }

    public string NumeroProtocolo { get; set; }
    public string Tipo { get; set; }
    public string DataProtocolo { get; set; }
    public string DataProcessamento { get; set; }
    public string Requisitante { get; set; }
}

public class Pauta
{
    public Pauta(string data, string hora, string orgaoJulgamento)
    {
        Data = data;
        Hora = hora;
        OrgaoJulgamento = orgaoJulgamento;
    }

    public string Data { get; set; }
    public string Hora { get; set; }
    public string OrgaoJulgamento { get; set; }
}
