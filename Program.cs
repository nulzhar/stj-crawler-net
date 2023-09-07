// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;

Console.WriteLine("Hello, World!");

using (var driver = UndetectedChromeDriver.Create(
    driverExecutablePath: @"/home/nulzhar/Público/chromedriver_linux64/chromedriver"))
{
    driver.GoToUrl("https://processo.stj.jus.br/processo/pesquisa/?aplicacao=processos.ea");
    Thread.Sleep(1000);

    // ##############################
    // # BUSCA PELA PARTE
    // ##############################

    var element = driver.FindElement(By.Id("idInterfaceVisualAreaBlocoInterno"));
    if (element.Text.ToLower().Contains("sistema indisponível"))
    {
        Console.WriteLine("SISTEMA INDISPONIVEL");
    }

    element = driver.FindElement(By.Id("idParteNome"));
    element.SendKeys("Banco Itau");

    element = driver.FindElement(By.Id("idBotaoPesquisarFormularioExtendido"));
    element.Click();

    // ##############################
    // # Seleciona todas as ref de parte
    // ##############################

    element = driver.FindElement(By.Id("idBotaoMarcarTodos"));
    element.Click();
    try
    {
        element = driver.FindElement(By.Id("idBotaoPesquisarMarcados"));
        element.Click();
    }
    catch (System.Exception)
    {
    }

    // ##############################
    // # AGUARDA O BLOCO DE MENSAGEM
    // ##############################

    try
    {
        element = new WebDriverWait(driver, TimeSpan.FromSeconds(600))
        .Until(drv => drv.FindElement(By.Id("idDivBlocoMensagem")));
    }
    catch (Exception ex)
    {

    }

    var scrollPauseTime = 20;
    var lastHeight = driver.ExecuteScript("return document.body.scrollHeight");
    while(true)
    {
        driver.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        Thread.Sleep(scrollPauseTime);

        var newHeight = driver.ExecuteScript("return document.body.scrollHeight");

        if ((long)newHeight == (long)lastHeight)
        {
            break;
        }

        lastHeight = newHeight;
    }


    element = driver.FindElement(By.Id("idBotaoCopiarColarCsv"));
    element.Click();
    
    IAlert alert = driver.SwitchTo().Alert();
    alert.Accept();
    
    string csv = "";
    try
    {
        element = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
        .Until(drv => drv.FindElement(By.Id("idCopiarParaCSV")));
        csv = element.Text;
    }
    catch (Exception ex)
    {

    }
    
    using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"/home/nulzhar/Público/", "WriteLines.txt")))
    {
        outputFile.WriteLine(csv);
    }
}