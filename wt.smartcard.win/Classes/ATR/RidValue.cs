using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class RidValue
    {
        public string Name { get; }
        public byte[] Rid { get; }

        static RidValue()
        {
            RidValue.RidValues = new[]
            {
                new RidValue("ACG (AG) Identification", "D276000095"),
                new RidValue("ACG Smartgate", "D276000091"),
                new RidValue("AmaTech", "D276000113"),
                new RidValue("AOK Leipzig", "D276000043"),
                new RidValue("APCON", "D276000023"),
                new RidValue("ARCOR", "D276000070"),
                new RidValue("ATRON", "D276000065"),
                new RidValue("AutoMeter", "D276000026"),
                new RidValue("AVS", "D276000131"),
                new RidValue("Bank-Verlag", "D276000074"),
                new RidValue("BAPT/BSI", "D276000066"),
                new RidValue("Bayer AG", "D276000058"),
                new RidValue("Beckmann", "D276000088"),
                new RidValue("Bek", "D276000016"),
                new RidValue("BEWATEC", "D276000071"),
                new RidValue("BGS Systemplanung", "D276000110"),
                new RidValue("Blaupunkt-Werke", "D276000017"),
                new RidValue("Bosch Telecom �V", "D276000010"),
                new RidValue("Bosch Telecom UC VT", "D276000006"),
                new RidValue("BSI", "D276000132"),
                new RidValue("BUILT", "D276000086"),
                new RidValue("Bull", "D276000073"),
                new RidValue("Bundes�rztekammer", "D276000146"),
                new RidValue("BZA", "D276000084"),
                new RidValue("Canoa", "D276000136"),
                new RidValue("card.etc", "D276000108"),
                new RidValue("Cards&Devices", "D276000045"),
                new RidValue("CCS", "D276000072"),
                new RidValue("Celectronic", "D276000018"),
                new RidValue("Celo Communications", "D276000101"),
                new RidValue("CE Infosys", "D276000079"),
                new RidValue("Challenge Card Design", "D276000051"),
                new RidValue("China Handel", "D276000053"),
                new RidValue("Cichon", "D276000094"),
                new RidValue("CogniMed", "D276000122"),
                new RidValue("Competition", "D276000061"),
                new RidValue("Compris Intelligence", "D276000137"),
                new RidValue("Contidata", "D276000029"),
                new RidValue("COPILOT", "D276000020"),
                new RidValue("Cryptovision", "D276000098"),
                new RidValue("DataCard", "D276000048"),
                new RidValue("DATEV", "D276000077"),
                new RidValue("DentCard", "D276000075"),
                new RidValue("DeTeMobil", "D276000047"),
                new RidValue("Deutsche Telekom, FTZ", "D276000052"),
                new RidValue("Deutsche Telek., Telesec", "D276000003"),
                new RidValue("Deutscher Sparkassenverlag", "D276000152"),
                new RidValue("DG-Verlag", "D276000067"),
                new RidValue("DGN-Service", "D276000127"),
                new RidValue("DRK", "D276000054"),
                new RidValue("DSGV", "D276000099"),
                new RidValue("EBS", "D276000033"),
                new RidValue("ECM", "D276000114"),
                new RidValue("ELGEBA", "D276000041"),
                new RidValue("ELME", "D276000111"),
                new RidValue("Endergonic", "D276000092"),
                new RidValue("EXAKT", "D276000120"),
                new RidValue("FlexoCard", "D276000129"),
                new RidValue("Fraunhofer SIT", "D276000007"),
                new RidValue("fsfEurope", "D276000124"),
                new RidValue("gematik", "D276000144"),
                new RidValue("Gemplus", "D276000035"),
                new RidValue("Gesetzl. KrankenV.", "D276000001"),
                new RidValue("Getronik", "D276000011"),
                new RidValue("Giesecke&Devrient", "D276000005"),
                new RidValue("Giesecke&Devrient2", "D276000118"),
                new RidValue("Globetac Solutions", "D276000140"),
                new RidValue("GM Consult IT", "D276000142"),
                new RidValue("gt german telematics", "D276000147"),
                new RidValue("Hagenuk", "D276000030"),
                new RidValue("Hess", "D276000064"),
                new RidValue("Holtkamp", "D276000008"),
                new RidValue("HTS", "D276000044"),
                new RidValue("IBM Laboratories", "D276000022"),
                new RidValue("IDpendant", "D276000155"),
                new RidValue("IGEL Technology", "D276000139"),
                new RidValue("IICS", "D276000138"),
                new RidValue("IKK Nordrhein", "D276000150"),
                new RidValue("Infineon", "D276000004"),
                new RidValue("Intercard GmbH Kartensysteme", "D276000158"),
                new RidValue("InterComponentWare AG", "D276000141"),
                new RidValue("Interflex", "D276000057"),
                new RidValue("ISS", "D276000109"),
                new RidValue("Kahlo", "D276000093"),
                new RidValue("Kaste Systemcard", "D276000012"),
                new RidValue("Kaste Zahlungssysteme", "D276000069"),
                new RidValue("Kobil", "D276000105"),
                new RidValue("KODAK", "D276000015"),
                new RidValue("Logics Software", "D276000102"),
                new RidValue("Lufthansa", "D276000024"),
                new RidValue("MARALU", "D276000049"),
                new RidValue("Masktech", "D276000126"),
                new RidValue("Medizon", "D276000115"),
                new RidValue("Meiller ComCard", "D276000027"),
                new RidValue("Mercedes Benz", "D276000031"),
                new RidValue("metabit", "D276000133"),
                new RidValue("M�hlbauer", "D276000153"),
                new RidValue("MW-Lotto", "D276000039"),
                new RidValue("My-tronic", "D276000112"),
                new RidValue("M+S V.- u. S.-Techn.", "D276000019"),
                new RidValue("Novacard", "D276000009"),
                new RidValue("NOVO", "D276000059"),
                new RidValue("NXP Semiconductors", "D276000085"),
                new RidValue("ODS", "D276000046"),
                new RidValue("One Smart World", "D276000117"),
                new RidValue("OSPT", "D276000156"),
                new RidValue("Ostsee-Tourismus", "D276000128"),
                new RidValue("PAV-CARD", "D276000063"),
                new RidValue("Physikalisch-Techn. Bundesanstalt (PTB)", "D276000148"),
                new RidValue("PKV", "D276000056"),
                new RidValue("Plail", "D276000068"),
                new RidValue("PPC Card Systems", "D276000089"),
                new RidValue("PROMEC", "D276000087"),
                new RidValue("Rankl", "D276000060"),
                new RidValue("REA Cards", "D276000145"),
                new RidValue("RIS-Software", "D276000151"),
                new RidValue("RMV", "D276000096"),
                new RidValue("SAGEM ORGA", "D276000028"),
                new RidValue("Scheidt & Bachmann", "D276000021"),
                new RidValue("SDS - Security Data Systems ", "D276000163"),
                new RidValue("Seca Vogel&Halke", "D276000100"),
                new RidValue("SECUDE", "D276000083"),
                new RidValue("SEFIROT", "D276000134"),
                new RidValue("Siemens A&D", "D276000106"),
                new RidValue("Siemens ANL", "D276000055"),
                new RidValue("Siemens HL", "D276000004"),
                new RidValue("Siemens ICN NES", "D276000080"),
                new RidValue("Siemens SI", "D276000014"),
                new RidValue("Siemens VT", "D276000034"),
                new RidValue("SI-Elektronik", "D276000078"),
                new RidValue("SII", "D276000121"),
                new RidValue("SimonsVoss Technologies", "D276000161"),
                new RidValue("SMC-Trust", "D276000130"),
                new RidValue("SNI", "D276000038"),
                new RidValue("Softways", "D276000160"),
                new RidValue("SPS", "D276000090"),
                new RidValue("Staatl. Lotterie Bayern", "D276000037"),
                new RidValue("sysmocom-systems for mobile communications", "D276000157"),
                new RidValue("Swissbit Germany", "D276000162"),
                new RidValue("Swiss Post Solutions", "D276000116"),
                new RidValue("Systemform", "D276000050"),
                new RidValue("Tally", "D276000103"),
                new RidValue("TELETEACH", "D276000107"),
                new RidValue("TeleTrusT", "D276000036"),
                new RidValue("TU Berlin", "D276000119"),
                new RidValue("TU Darmstadt", "D276000042"),
                new RidValue("TU Dresden", "D276000149"),
                new RidValue("TOWITOKO", "D276000062"),
                new RidValue("UniKiel", "D276000123"),
                new RidValue("Union Tank", "D276000082"),
                new RidValue("UITIMACO", "D276000076"),
                new RidValue("Vereinigte IKK", "D276000154"),
                new RidValue("VDV", "D276000135"),
                new RidValue("VEGAS", "D276000081"),
                new RidValue("V�B-ZVD", "D276000104"),
                new RidValue("Web and Cards", "D276000143"),
                new RidValue("Wincor Nixdorf", "D276000125"),
                new RidValue("Winter Wertdruck", "D276000032"),
                new RidValue("ZeitControl", "D276000002"),
                new RidValue("ZI der KV", "D276000040"),
                new RidValue("ZKA", "D276000025")
            };
        }

        public static readonly RidValue[] RidValues;

        private RidValue(string name, string rid)
        {
            this.Name = name;
            this.Rid = rid.ToByteArray();
        }

        public static RidValue GetFromRid(byte[] rid)
        {
            return RidValue.RidValues.FirstOrDefault(_ => _.Rid.HasEqualValue(rid));
        }

        public override string ToString()
        {
            return $"{this.Name} ({this.Rid.ToHexString()})";
        }
    }
}