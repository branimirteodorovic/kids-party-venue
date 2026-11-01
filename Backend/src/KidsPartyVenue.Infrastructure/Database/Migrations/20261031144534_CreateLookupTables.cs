using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KidsPartyVenue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "amenities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_sr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name_ru = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_amenities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_sr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name_ru = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    locative_name_sr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    locative_name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    locative_name_ru = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    center = table.Column<Point>(type: "geography (Point, 4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "city_categories",
                columns: table => new
                {
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    opened_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_city_categories", x => new { x.city_id, x.category });
                    table.ForeignKey(
                        name: "fk_city_categories_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_sr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name_ru = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_districts", x => x.id);
                    table.ForeignKey(
                        name: "fk_districts_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "amenities",
                columns: new[] { "id", "slug", "sort_order", "name_en", "name_ru", "name_sr" },
                values: new object[,]
                {
                    { new Guid("01a19263-5b30-722c-9614-c585e4588a18"), "parking", 1, "Parking", "Парковка", "Parking" },
                    { new Guid("01a19263-5b30-722c-9614-c5865d500eb1"), "klima-uredjaj", 2, "Air conditioning", "Кондиционер", "Klima uređaj" },
                    { new Guid("01a19263-5b30-722c-9614-c58760a30118"), "wi-fi", 3, "Wi-Fi", "Wi-Fi", "Wi-Fi" },
                    { new Guid("01a19263-5b30-722c-9614-c58804ab5580"), "dvoriste", 4, "Outdoor area", "Открытая площадка", "Dvorište" },
                    { new Guid("01a19263-5b30-722c-9614-c589ec244a62"), "kafic-za-roditelje", 5, "Café for parents", "Кафе для родителей", "Kafić za roditelje" },
                    { new Guid("01a19263-5b30-722c-9614-c58afd9c21d4"), "prilaz-za-kolica", 6, "Stroller access", "Доступ для колясок", "Prilaz za kolica" },
                    { new Guid("01a19263-5b30-722c-9614-c58bfd4b59c2"), "sto-za-presvlacenje-beba", 7, "Baby changing table", "Пеленальный столик", "Sto za presvlačenje beba" },
                    { new Guid("01a19263-5b31-7634-a272-560179ad2912"), "kutak-za-bebe", 8, "Baby corner", "Уголок для малышей", "Kutak za bebe" },
                    { new Guid("01a19263-5b31-7634-a272-5602e6767e12"), "trampolina", 9, "Trampoline", "Батут", "Trampolina" },
                    { new Guid("01a19263-5b31-7634-a272-5603cd0d8972"), "lavirint", 10, "Play maze", "Лабиринт", "Lavirint" },
                    { new Guid("01a19263-5b31-7634-a272-5604c3f275c6"), "bazen-sa-lopticama", 11, "Ball pit", "Сухой бассейн", "Bazen sa lopticama" },
                    { new Guid("01a19263-5b31-7634-a272-56053d1817c0"), "ozvucenje", 12, "Sound system", "Звуковая система", "Ozvučenje" },
                    { new Guid("01a19263-5b31-7634-a272-56062fde9c2e"), "projektor", 13, "Projector", "Проектор", "Projektor" }
                });

            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "id", "center", "slug", "name_en", "name_ru", "name_sr" },
                values: new object[,]
                {
                    { new Guid("01a19263-5b0f-7065-80f4-2731fca7e89d"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.12222 45.80139)"), "ada", null, "Ада", "Ada" },
                    { new Guid("01a19263-5b0f-7065-80f4-2732c910e6a9"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.05139 43.45528)"), "aleksandrovac", null, "Александровац", "Aleksandrovac" },
                    { new Guid("01a19263-5b0f-7065-80f4-273364bdec6e"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.70472 43.53833)"), "aleksinac", null, "Алексинац", "Aleksinac" },
                    { new Guid("01a19263-5b0f-7065-80f4-27344658445d"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.96583 45.08083)"), "alibunar", null, "Алибунар", "Alibunar" },
                    { new Guid("01a19263-5b0f-7065-80f4-273579a88fa6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (18.98333 45.66667)"), "apatin", null, "Апатин", "Apatin" },
                    { new Guid("01a19263-5b10-76bf-9bb2-c2a730b49cfc"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.55611 44.30417)"), "arandjelovac", null, "Аранджеловац", "Aranđelovac" },
                    { new Guid("01a19263-5b10-76bf-9bb2-c2a851941c07"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.09056 43.75194)"), "arilje", null, "Арилье", "Arilje" },
                    { new Guid("01a19263-5b10-76bf-9bb2-c2a983af98a8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.40139 43.05583)"), "babusnica", null, "Бабушница", "Babušnica" },
                    { new Guid("01a19263-5b10-76bf-9bb2-c2aa63402971"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.23528 45.38861)"), "bac", null, "Бач", "Bač" },
                    { new Guid("01a19263-5b10-76bf-9bb2-c2ab75356081"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.38861 45.25056)"), "backa-palanka", null, "Бачка-Паланка", "Bačka Palanka" },
                    { new Guid("01a19263-5b11-74bb-8979-85a359642053"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.635 45.80889)"), "backa-topola", null, "Бачка-Топола", "Bačka Topola" },
                    { new Guid("01a19263-5b11-74bb-8979-85a43ecd33ff"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.58833 45.35639)"), "backi-petrovac", null, "Бачки-Петровац", "Bački Petrovac" },
                    { new Guid("01a19263-5b11-74bb-8979-85a5ccf7601f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.55972 43.97306)"), "bajina-basta", null, "Баина-Башта", "Bajina Bašta" },
                    { new Guid("01a19263-5b11-74bb-8979-85a68e458d0b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.08333 44.15)"), "batocina", null, "Баточина", "Batočina" },
                    { new Guid("01a19263-5b11-74bb-8979-85a7b1e8619b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.03333 45.61667)"), "becej", null, "Бечей", "Bečej" },
                    { new Guid("01a19263-5b11-74bb-8979-85a8c2e3bf7f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.41722 44.8975)"), "bela-crkva", null, "Бела-Црква", "Bela Crkva" },
                    { new Guid("01a19263-5b11-74bb-8979-85a929278940"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.30667 43.21778)"), "bela-palanka", null, "Бела-Паланка", "Bela Palanka" },
                    { new Guid("01a19263-5b11-74bb-8979-85aa6542aa1a"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.72028 45.19222)"), "beocin", null, "Беочин", "Beočin" },
                    { new Guid("01a19263-5b11-74bb-8979-85ab84c1838d"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.45694 44.81778)"), "beograd", "Belgrade", "Белград", "Beograd" },
                    { new Guid("01a19263-5b11-74bb-8979-85acc5127449"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.28472 43.29056)"), "blace", null, "Блаце", "Blace" },
                    { new Guid("01a19263-5b12-70c7-bb33-50ea06268b9a"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.48333 44.83333)"), "bogatic", null, "Богатич", "Bogatić" },
                    { new Guid("01a19263-5b12-70c7-bb33-50ebb70f423d"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.73333 43.01667)"), "bojnik", null, "Бойник", "Bojnik" },
                    { new Guid("01a19263-5b12-70c7-bb33-50ec23455771"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.9531 43.8303)"), "boljevac", null, "Болевац", "Boljevac" },
                    { new Guid("01a19263-5b12-70c7-bb33-50ed9ff8ff96"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.10361 44.13028)"), "bor", null, "Бор", "Bor" },
                    { new Guid("01a19263-5b12-70c7-bb33-50eea66a6e68"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.46667 42.5)"), "bosilegrad", null, "Босилеград", "Bosilegrad" },
                    { new Guid("01a19263-5b12-70c7-bb33-50ef1f38d207"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.03361 43.38361)"), "brus", null, "Брус", "Brus" },
                    { new Guid("01a19263-5b12-70c7-bb33-50f0eae4bba4"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.76667 42.46667)"), "bujanovac", null, "Буяновац", "Bujanovac" },
                    { new Guid("01a19263-5b12-70c7-bb33-50f1147b8ffa"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.34972 43.89139)"), "cacak", null, "Чачак", "Čačak" },
                    { new Guid("01a19263-5b12-70c7-bb33-50f277768086"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.71667 43.75)"), "cajetina", null, "Чаетина", "Čajetina" },
                    { new Guid("01a19263-5b12-70c7-bb33-50f324a5052b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.45 43.71667)"), "cicevac", null, "Чичевац", "Ćićevac" },
                    { new Guid("01a19263-5b12-70c7-bb33-50f4f5c1ff82"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.13944 45.93889)"), "coka", null, "Чока", "Čoka" },
                    { new Guid("01a19263-5b13-72f8-aa37-f5269acfa831"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.29901 42.81015)"), "crna-trava", null, "Црна-Трава", "Crna Trava" },
                    { new Guid("01a19263-5b13-72f8-aa37-f527bbb23f9e"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.36861 43.92306)"), "cuprija", null, "Чуприя", "Ćuprija" },
                    { new Guid("01a19263-5b13-72f8-aa37-f528e9e453f3"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.28333 42.53333)"), "decani", null, "Дечани", "Dečani" },
                    { new Guid("01a19263-5b13-72f8-aa37-f529da76e8d0"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.43333 44.08333)"), "despotovac", null, "Деспотовац", "Despotovac" },
                    { new Guid("01a19263-5b13-72f8-aa37-f52ab88830b8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.78333 43.01667)"), "dimitrovgrad", null, "Димитровград", "Dimitrovgrad" },
                    { new Guid("01a19263-5b13-72f8-aa37-f52b2e3f8e20"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.43556 42.3825)"), "djakovica", null, "Джаковица", "Đakovica" },
                    { new Guid("01a19263-5b13-72f8-aa37-f52c587c81cf"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.8331 43.19892)"), "doljevac", null, "Долевац", "Doljevac" },
                    { new Guid("01a19263-5b13-72f8-aa37-f52d823b75d8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.02583 43.22028)"), "gadzin-han", null, "Гаджин-Хан", "Gadžin Han" },
                    { new Guid("01a19263-5b13-72f8-aa37-f52e09370587"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.89389 42.62833)"), "glogovac", null, "Глоговац", "Glogovac" },
                    { new Guid("01a19263-5b14-77c0-b018-7ce06537c0cb"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.46667 42.46667)"), "gnjilane", null, "Гнилане", "Gnjilane" },
                    { new Guid("01a19263-5b14-77c0-b018-7ce1af487788"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.63333 44.65)"), "golubac", null, "Голубац", "Golubac" },
                    { new Guid("01a19263-5b15-77e0-8122-fd6a665c1956"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.65315 42.06142)"), "gora", null, "Гора", "Gora" },
                    { new Guid("01a19263-5b15-77e0-8122-fd6b35cf1dd3"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.45595 44.02115)"), "gornji-milanovac", null, "Горни-Милановац", "Gornji Milanovac" },
                    { new Guid("01a19263-5b15-77e0-8122-fd6c0ea44a17"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.07917 45.04917)"), "indjija", null, "Инджия", "Inđija" },
                    { new Guid("01a19263-5b15-77e0-8122-fd6de1770336"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.85833 45.10111)"), "irig", null, "Ириг", "Irig" },
                    { new Guid("01a19263-5b16-755e-8004-124bec326d78"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.48333 42.78333)"), "istok", null, "Исток", "Istok" },
                    { new Guid("01a19263-5b16-755e-8004-124c3e7deb68"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.22972 43.58111)"), "ivanjica", null, "Иваница", "Ivanjica" },
                    { new Guid("01a19263-5b16-755e-8004-124da076c883"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.25639 43.975)"), "jagodina", null, "Ягодина", "Jagodina" },
                    { new Guid("01a19263-5b16-755e-8004-124ea0000654"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.25944 42.23194)"), "kacanik", null, "Качаник", "Kačanik" },
                    { new Guid("01a19263-5b16-755e-8004-124fb0f3071b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.05 46.06667)"), "kanjiza", null, "Канижа", "Kanjiža" },
                    { new Guid("01a19263-5b16-755e-8004-12501ae1db47"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.45917 45.82444)"), "kikinda", null, "Кикинда", "Kikinda" },
                    { new Guid("01a19263-5b16-755e-8004-1251e10c3bba"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.60722 44.60389)"), "kladovo", null, "Кладово", "Kladovo" },
                    { new Guid("01a19263-5b17-72f2-9124-2c01c319f322"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.57778 42.62167)"), "klina", null, "Клина", "Klina" },
                    { new Guid("01a19263-5b17-72f2-9124-2c0267177cfc"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.71667 43.91667)"), "knic", null, "Книч", "Knić" },
                    { new Guid("01a19263-5b17-72f2-9124-2c0306f2619a"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.255 43.567)"), "knjazevac", null, "Княжевац", "Knjaževac" },
                    { new Guid("01a19263-5b17-72f2-9124-2c04ff53dff8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.80694 44.47083)"), "koceljeva", null, "Коцелева", "Koceljeva" },
                    { new Guid("01a19263-5b17-72f2-9124-2c051f232cc1"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.91667 44)"), "kosjeric", null, "Косерич", "Kosjerić" },
                    { new Guid("01a19263-5b17-72f2-9124-2c068beadae2"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.0975 42.63833)"), "kosovo-polje", null, "Косово-Полье", "Kosovo Polje" },
                    { new Guid("01a19263-5b17-72f2-9124-2c07676f22ee"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.56667 42.56667)"), "kosovska-kamenica", null, "Косовска-Каменица", "Kosovska Kamenica" },
                    { new Guid("01a19263-5b17-72f2-9124-2c08ffa451ee"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.86667 42.88333)"), "kosovska-mitrovica", null, "Косовска-Митровица", "Kosovska Mitrovica" },
                    { new Guid("01a19263-5b17-72f2-9124-2c09898cc955"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.62139 45.11167)"), "kovacica", null, "Ковачица", "Kovačica" },
                    { new Guid("01a19263-5b17-72f2-9124-2c0a8a341af4"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.97611 44.7475)"), "kovin", null, "Ковин", "Kovin" },
                    { new Guid("01a19263-5b17-72f2-9124-2c0bb27201d9"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.91722 44.01011)"), "kragujevac", null, "Крагуевац", "Kragujevac" },
                    { new Guid("01a19263-5b18-76d2-9350-2000538dff3f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.68698 43.72342)"), "kraljevo", null, "Кралево", "Kraljevo" },
                    { new Guid("01a19263-5b18-76d2-9350-200149c78f51"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.36194 44.36556)"), "krupanj", null, "Крупань", "Krupanj" },
                    { new Guid("01a19263-5b18-76d2-9350-200280ae1bb9"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.32667 43.58333)"), "krusevac", null, "Крушевац", "Kruševac" },
                    { new Guid("01a19263-5b18-76d2-9350-2003d0341a64"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.66667 44.48333)"), "kucevo", null, "Кучево", "Kučevo" },
                    { new Guid("01a19263-5b18-76d2-9350-20049d149ca4"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.53333 45.6)"), "kula", null, "Кула", "Kula" },
                    { new Guid("01a19263-5b18-76d2-9350-20058eeb391c"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.26778 43.14083)"), "kursumlija", null, "Куршумлия", "Kuršumlija" },
                    { new Guid("01a19263-5b18-76d2-9350-2006dc136b19"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.16667 44.36667)"), "lajkovac", null, "Лайковац", "Lajkovac" },
                    { new Guid("01a19263-5b18-76d2-9350-2007ef396952"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.09727 44.18424)"), "lapovo", null, "Лапово", "Lapovo" },
                    { new Guid("01a19263-5b18-76d2-9350-20088fe17753"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.73333 42.91667)"), "lebane", null, "Лебане", "Lebane" },
                    { new Guid("01a19263-5b18-76d2-9350-20091a0223a9"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.8 43.1)"), "leposavic", null, "Лепосавич", "Leposavić" },
                    { new Guid("01a19263-5b18-76d2-9350-200a2061e383"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.94611 42.99806)"), "leskovac", null, "Лесковац", "Leskovac" },
                    { new Guid("01a19263-5b18-76d2-9350-200bee6cfdc1"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.12583 42.52167)"), "lipljan", null, "Липлян", "Lipljan" },
                    { new Guid("01a19263-5b18-76d2-9350-200c953843ec"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.2375 44.22133)"), "ljig", null, "Лиг", "Ljig" },
                    { new Guid("01a19263-5b19-73c4-841c-909f09f58fb1"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.37581 44.18922)"), "ljubovija", null, "Любовия", "Ljubovija" },
                    { new Guid("01a19263-5b19-73c4-841c-90a045cf0288"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.22583 44.53333)"), "loznica", null, "Лозница", "Loznica" },
                    { new Guid("01a19263-5b19-73c4-841c-90a18502da76"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.13333 43.86667)"), "lucani", null, "Лучани", "Lučani" },
                    { new Guid("01a19263-5b19-73c4-841c-90a2dca18129"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.93333 44.41667)"), "majdanpek", null, "Майданпек", "Majdanpek" },
                    { new Guid("01a19263-5b19-73c4-841c-90a36fea79f3"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.66444 45.70694)"), "mali-idjos", null, "Мали-Иджош", "Mali Iđoš" },
                    { new Guid("01a19263-5b19-73c4-841c-90a4da00c84b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.10505 44.3727)"), "mali-zvornik", null, "Мали-Зворник", "Mali Zvornik" },
                    { new Guid("01a19263-5b19-73c4-841c-90a560e3c1e5"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.28333 44.56667)"), "malo-crnice", null, "Мало-Црниче", "Malo Crniće" },
                    { new Guid("01a19263-5b19-73c4-841c-90a68d22dd46"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.58333 42.83333)"), "medvedja", null, "Медведжа", "Medveđa" },
                    { new Guid("01a19263-5b19-73c4-841c-90a7939376c6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.71667 43.28333)"), "merosina", null, "Мерошина", "Merošina" },
                    { new Guid("01a19263-5b19-73c4-841c-90a8e66d431b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.08333 44.25)"), "mionica", null, "Мионица", "Mionica" },
                    { new Guid("01a19263-5b19-73c4-841c-90a9d24acaeb"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.51667 44.21667)"), "negotin", null, "Неготин", "Negotin" },
                    { new Guid("01a19263-5b19-73c4-841c-90aa36ed7d91"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.89611 43.31917)"), "nis", null, "Ниш", "Niš" },
                    { new Guid("01a19263-5b1a-7614-a6da-830139f78eaf"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.6 45.66667)"), "nova-crnja", null, "Нова-Црня", "Nova Crnja" },
                    { new Guid("01a19263-5b1a-7614-a6da-8302e01563b7"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.82028 43.46667)"), "nova-varos", null, "Нова-Варош", "Nova Varoš" },
                    { new Guid("01a19263-5b1a-7614-a6da-83033e432ceb"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.11667 45.6)"), "novi-becej", null, "Нови-Бечей", "Novi Bečej" },
                    { new Guid("01a19263-5b1a-7614-a6da-830451fb5ecf"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.1 46.05)"), "novi-knezevac", null, "Нови-Кнежевац", "Novi Kneževac" },
                    { new Guid("01a19263-5b1a-7614-a6da-8305628d19bc"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.51667 43.15)"), "novi-pazar", null, "Нови-Пазар", "Novi Pazar" }
                });

            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "id", "center", "slug", "locative_name_en", "locative_name_ru", "locative_name_sr", "name_en", "name_ru", "name_sr" },
                values: new object[] { new Guid("01a19263-5b1a-7614-a6da-83063950107f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.83167 45.26444)"), "novi-sad", "Novi Sad", "Нови-Саде", "Novom Sadu", null, "Нови-Сад", "Novi Sad" });

            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "id", "center", "slug", "name_en", "name_ru", "name_sr" },
                values: new object[,]
                {
                    { new Guid("01a19263-5b1c-717c-b578-b1cf2a025849"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.4261 42.6014)"), "novo-brdo", null, "Ново-Брдо", "Novo Brdo" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d0f7a89bf2"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.07778 42.69)"), "obilic", null, "Обилич", "Obilić" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d137614879"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.26667 45.51667)"), "odzaci", null, "Оджаци", "Odžaci" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d28e7a9ec3"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.42472 45.05139)"), "opovo", null, "Опово", "Opovo" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d380da0627"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.65472 42.39944)"), "orahovac", null, "Ораховац", "Orahovac" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d4e6c18545"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.6 44.36667)"), "osecina", null, "Осечина", "Osečina" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d557b227e8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.65194 44.87389)"), "pancevo", null, "Панчево", "Pančevo" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d65f56adc2"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.41667 43.86667)"), "paracin", null, "Парачин", "Paraćin" },
                    { new Guid("01a19263-5b1c-717c-b578-b1d7d8988cc6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.28828 42.65913)"), "pec", null, "Печ", "Peć" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87f9c06a0d50"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.96639 44.90889)"), "pecinci", null, "Печинци", "Pećinci" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87faba253469"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.41944 44.37833)"), "petrovac-na-mlavi", null, "Петровац-на-Млави", "Petrovac na Mlavi" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87fbd6b897ec"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.585 43.15194)"), "pirot", null, "Пирот", "Pirot" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87fca828d402"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.12167 45.22694)"), "plandiste", null, "Пландиште", "Plandište" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87fdc7a37e84"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.19167 42.91167)"), "podujevo", null, "Подуево", "Podujevo" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87fe1a96454e"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.18972 44.62)"), "pozarevac", null, "Пожаревац", "Požarevac" },
                    { new Guid("01a19263-5b1d-77e8-8e25-87ff97466d04"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.05 43.85)"), "pozega", null, "Пожега", "Požega" },
                    { new Guid("01a19263-5b1d-77e8-8e25-8800073e2650"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.65 42.30667)"), "presevo", null, "Прешево", "Preševo" },
                    { new Guid("01a19263-5b1d-77e8-8e25-880144965efe"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.52734 43.58163)"), "priboj", null, "Прибой", "Priboj" },
                    { new Guid("01a19263-5b1d-77e8-8e25-8802b6b9e741"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.65139 43.54389)"), "prijepolje", null, "Приеполье", "Prijepolje" },
                    { new Guid("01a19263-5b1e-7585-b890-03519dc3fca2"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.16667 42.66667)"), "pristina", null, "Приштина", "Priština" },
                    { new Guid("01a19263-5b1e-7585-b890-03520b18f155"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.74 42.23)"), "prizren", null, "Призрен", "Prizren" },
                    { new Guid("01a19263-5b1e-7585-b890-0353d34d8540"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.58611 43.23389)"), "prokuplje", null, "Прокуплье", "Prokuplje" },
                    { new Guid("01a19263-5b23-77c3-a38a-1e4d7562e023"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.98333 44.23333)"), "raca", null, "Рача", "Rača" },
                    { new Guid("01a19263-5b23-77c3-a38a-1e4ed0dab1ee"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.60917 43.28556)"), "raska", null, "Рашка", "Raška" },
                    { new Guid("01a19263-5b23-77c3-a38a-1e4f096f5fb6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.55 43.66667)"), "razanj", null, "Ражань", "Ražanj" },
                    { new Guid("01a19263-5b24-744f-aada-0d76f1722abd"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.13333 43.86667)"), "rekovac", null, "Рековац", "Rekovac" },
                    { new Guid("01a19263-5b24-744f-aada-0d771f1ab4dd"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.82889 45.00306)"), "ruma", null, "Рума", "Ruma" },
                    { new Guid("01a19263-5b24-744f-aada-0d78fc8301be"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.69389 44.75583)"), "sabac", null, "Шабац", "Šabac" },
                    { new Guid("01a19263-5b24-744f-aada-0d7977f921af"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.77222 45.36667)"), "secanj", null, "Сечань", "Sečanj" },
                    { new Guid("01a19263-5b24-744f-aada-0d7ad7c8f008"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.09 45.93139)"), "senta", null, "Сента", "Senta" },
                    { new Guid("01a19263-5b24-744f-aada-0d7bbbc818a6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.22639 45.12833)"), "sid", null, "Шид", "Šid" },
                    { new Guid("01a19263-5b24-744f-aada-0d7c00b6b6c1"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20 43.26667)"), "sjenica", null, "Сеница", "Sjenica" },
                    { new Guid("01a19263-5b24-744f-aada-0d7d762e9a64"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.92889 44.66333)"), "smederevo", null, "Смедерево", "Smederevo" },
                    { new Guid("01a19263-5b24-744f-aada-0d7ebcacd163"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.95867 44.3655)"), "smederevska-palanka", null, "Смедеревска-Паланка", "Smederevska Palanka" },
                    { new Guid("01a19263-5b24-744f-aada-0d7fce658960"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.86944 43.63944)"), "sokobanja", null, "Сокобаня", "Sokobanja" },
                    { new Guid("01a19263-5b24-744f-aada-0d80e7aae15d"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.12 45.78)"), "sombor", null, "Сомбор", "Sombor" },
                    { new Guid("01a19263-5b24-744f-aada-0d81d195d3ad"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.78806 42.7475)"), "srbica", null, "Србица", "Srbica" },
                    { new Guid("01a19263-5b25-700d-a278-f45ea4edbef0"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.80167 45.55222)"), "srbobran", null, "Србобран", "Srbobran" },
                    { new Guid("01a19263-5b25-700d-a278-f45f099cff25"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.61011 44.96645)"), "sremska-mitrovica", null, "Сремска-Митровица", "Sremska Mitrovica" },
                    { new Guid("01a19263-5b25-700d-a278-f460fa1de4fa"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.93333 45.2)"), "sremski-karlovci", null, "Сремски-Карловци", "Sremski Karlovci" },
                    { new Guid("01a19263-5b25-700d-a278-f461455ee623"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.16667 44.98333)"), "stara-pazova", null, "Стара-Пазова", "Stara Pazova" },
                    { new Guid("01a19263-5b25-700d-a278-f46231065301"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.03333 42.43333)"), "stimlje", null, "Штимлье", "Štimlje" },
                    { new Guid("01a19263-5b25-700d-a278-f463ed27dc03"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.02722 42.23944)"), "strpce", null, "Штрпце", "Štrpce" },
                    { new Guid("01a19263-5b25-700d-a278-f46421776a64"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.66533 46.10021)"), "subotica", null, "Суботица", "Subotica" },
                    { new Guid("01a19263-5b25-700d-a278-f465aca17f4f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.16722 42.695)"), "surdulica", null, "Сурдулица", "Surdulica" },
                    { new Guid("01a19263-5b25-700d-a278-f46607e837ab"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.82194 42.38)"), "suva-reka", null, "Сува-Река", "Suva Reka" },
                    { new Guid("01a19263-5b25-700d-a278-f46764d8eabf"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.19473 44.23331)"), "svilajnac", null, "Свилайнац", "Svilajnac" },
                    { new Guid("01a19263-5b25-700d-a278-f46818805cda"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.11667 43.41667)"), "svrljig", null, "Сврлиг", "Svrljig" },
                    { new Guid("01a19263-5b25-700d-a278-f4699201c7da"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.88694 45.40528)"), "temerin", null, "Темерин", "Temerin" },
                    { new Guid("01a19263-5b26-748a-a06d-58ce1c8005e7"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.3 45.2)"), "titel", null, "Тител", "Titel" },
                    { new Guid("01a19263-5b26-748a-a06d-58cf2a8a44e2"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.67611 44.2525)"), "topola", null, "Топола", "Topola" },
                    { new Guid("01a19263-5b26-748a-a06d-58d0fd91f702"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.08321 42.36368)"), "trgoviste", null, "Трговиште", "Trgovište" },
                    { new Guid("01a19263-5b26-748a-a06d-58d1b5ea8120"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.99722 43.61861)"), "trstenik", null, "Трстеник", "Trstenik" },
                    { new Guid("01a19263-5b26-748a-a06d-58d23657dad0"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.32556 42.9875)"), "tutin", null, "Тутин", "Tutin" },
                    { new Guid("01a19263-5b26-748a-a06d-58d34cddbaa6"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.06667 44.45)"), "ub", null, "Уб", "Ub" },
                    { new Guid("01a19263-5b26-748a-a06d-58d42d14aa79"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.155 42.37)"), "urosevac", null, "Урошевац", "Uroševac" },
                    { new Guid("01a19263-5b26-748a-a06d-58d594251f87"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.85 43.85)"), "uzice", null, "Ужице", "Užice" },
                    { new Guid("01a19263-5b26-748a-a06d-58d6b90030cf"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.88333 44.26667)"), "valjevo", null, "Валево", "Valjevo" },
                    { new Guid("01a19263-5b26-748a-a06d-58d7cf99301b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.36667 43.71667)"), "varvarin", null, "Варварин", "Varvarin" },
                    { new Guid("01a19263-5b26-748a-a06d-58d8fdc00bd8"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.08333 44.33333)"), "velika-plana", null, "Велика-Плана", "Velika Plana" },
                    { new Guid("01a19263-5b27-7759-af85-93150ec7f6e4"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.51667 44.75)"), "veliko-gradiste", null, "Велико-Градиште", "Veliko Gradište" },
                    { new Guid("01a19263-5b27-7759-af85-931657670c14"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.35833 42.32139)"), "vitina", null, "Витина", "Vitina" },
                    { new Guid("01a19263-5b27-7759-af85-9317fc1c5c61"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.05917 42.70778)"), "vladicin-han", null, "Владичин-Хан", "Vladičin Han" },
                    { new Guid("01a19263-5b27-7759-af85-931888f6e43f"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.78333 44.61667)"), "vladimirci", null, "Владимирци", "Vladimirci" },
                    { new Guid("01a19263-5b27-7759-af85-93190babf389"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.13333 42.96667)"), "vlasotince", null, "Власотинце", "Vlasotince" },
                    { new Guid("01a19263-5b27-7759-af85-931a2bf62b9a"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.89722 42.55417)"), "vranje", null, "Вранье", "Vranje" },
                    { new Guid("01a19263-5b27-7759-af85-931bcff4cc97"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (19.63778 45.56972)"), "vrbas", null, "Врбас", "Vrbas" },
                    { new Guid("01a19263-5b27-7759-af85-931cd7315836"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.9 43.61667)"), "vrnjacka-banja", null, "Врнячка-Баня", "Vrnjačka Banja" },
                    { new Guid("01a19263-5b27-7759-af85-931dbe3852fa"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.29861 45.12056)"), "vrsac", null, "Вршац", "Vršac" },
                    { new Guid("01a19263-5b27-7759-af85-931ef3982cfb"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.96667 42.82222)"), "vucitrn", null, "Вучитрн", "Vučitrn" },
                    { new Guid("01a19263-5b27-7759-af85-931fbe72f12b"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.06667 45.36667)"), "zabalj", null, "Жабаль", "Žabalj" },
                    { new Guid("01a19263-5b28-75ee-8018-b982d0b783d5"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.209 44.35367)"), "zabari", null, "Жабари", "Žabari" },
                    { new Guid("01a19263-5b28-75ee-8018-b983cdd25736"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.78933 44.19067)"), "zagubica", null, "Жагубица", "Žagubica" },
                    { new Guid("01a19263-5b28-75ee-8018-b98472288c18"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (22.28472 43.90417)"), "zajecar", null, "Заечар", "Zaječar" },
                    { new Guid("01a19263-5b28-75ee-8018-b985cbd9e2ff"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.54972 45.485)"), "zitiste", null, "Житиште", "Žitište" },
                    { new Guid("01a19263-5b28-75ee-8018-b986391062de"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (21.71667 43.18333)"), "zitoradja", null, "Житораджа", "Žitorađa" },
                    { new Guid("01a19263-5b28-75ee-8018-b98704e89e79"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.38611 45.37778)"), "zrenjanin", null, "Зренянин", "Zrenjanin" },
                    { new Guid("01a19263-5b28-75ee-8018-b9884bfe8244"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.68333 42.91667)"), "zubin-potok", null, "Зубин-Поток", "Zubin Potok" },
                    { new Guid("01a19263-5b28-75ee-8018-b9899942fc30"), (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (20.84 42.91)"), "zvecan", null, "Звечан", "Zvečan" }
                });

            migrationBuilder.InsertData(
                table: "districts",
                columns: new[] { "id", "city_id", "slug", "name_en", "name_ru", "name_sr" },
                values: new object[,]
                {
                    { new Guid("01a19263-5b2c-709e-8706-77eb260fee2d"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "centar", "City Centre", "Центр", "Centar" },
                    { new Guid("01a19263-5b2c-709e-8706-77ec1bdd37cf"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "liman", null, "Лиман", "Liman" },
                    { new Guid("01a19263-5b2c-709e-8706-77ed1140406d"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "grbavica", null, "Грбавица", "Grbavica" },
                    { new Guid("01a19263-5b2c-709e-8706-77eef1a0180e"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "adice", null, "Адице", "Adice" },
                    { new Guid("01a19263-5b2c-709e-8706-77ef9aaba0b1"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "telep", null, "Телеп", "Telep" },
                    { new Guid("01a19263-5b2c-709e-8706-77f0eae90268"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "detelinara", null, "Детелинара", "Detelinara" },
                    { new Guid("01a19263-5b2d-71c2-9405-39c61220dc37"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "novo-naselje", null, "Ново-Населье", "Novo naselje" },
                    { new Guid("01a19263-5b2d-71c2-9405-39c75f3ed123"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "satelit", null, "Сателит", "Satelit" },
                    { new Guid("01a19263-5b2d-71c2-9405-39c8c6aac9fa"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "podbara", null, "Подбара", "Podbara" },
                    { new Guid("01a19263-5b2d-71c2-9405-39c96e0e8d4a"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "salajka", null, "Салайка", "Salajka" },
                    { new Guid("01a19263-5b2d-71c2-9405-39ca5d0bad5f"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "rotkvarija", null, "Ротквария", "Rotkvarija" },
                    { new Guid("01a19263-5b2d-71c2-9405-39cb429281a5"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "klisa", null, "Клиса", "Klisa" },
                    { new Guid("01a19263-5b2d-71c2-9405-39cce766e02a"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "sajlovo", null, "Сайлово", "Sajlovo" },
                    { new Guid("01a19263-5b2d-71c2-9405-39cd004d2f5e"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "avijaticarsko-naselje", null, "Авиятичарско-Населье", "Avijatičarsko naselje" },
                    { new Guid("01a19263-5b2e-773b-8d54-15ce1515b936"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "petrovaradin", null, "Петроварадин", "Petrovaradin" },
                    { new Guid("01a19263-5b2e-773b-8d54-15cfdf9d2598"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "sremska-kamenica", null, "Сремска-Каменица", "Sremska Kamenica" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d0ca9d5bf4"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "veternik", null, "Ветерник", "Veternik" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d19f7a2e06"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "futog", null, "Футог", "Futog" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d2e480ce72"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "kac", null, "Кач", "Kać" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d377155d9b"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "budisava", null, "Будисава", "Budisava" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d4a1098a85"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "kovilj", null, "Ковиль", "Kovilj" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d5d06de373"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "ledinci", null, "Лединци", "Ledinci" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d6627ae33f"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "bukovac", null, "Буковац", "Bukovac" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d726a24676"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "rumenka", null, "Руменка", "Rumenka" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d84cc3dc5f"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "kisac", null, "Кисач", "Kisač" },
                    { new Guid("01a19263-5b2e-773b-8d54-15d9ef412867"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "cenej", null, "Ченей", "Čenej" },
                    { new Guid("01a19263-5b2e-773b-8d54-15da0d3eaab9"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "stepanovicevo", null, "Степановичево", "Stepanovićevo" },
                    { new Guid("01a19263-5b2e-773b-8d54-15db63de12df"), new Guid("01a19263-5b1a-7614-a6da-83063950107f"), "begec", null, "Бегеч", "Begeč" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_amenities_slug",
                table: "amenities",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cities_center",
                table: "cities",
                column: "center")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_cities_slug",
                table: "cities",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_districts_city_id_slug",
                table: "districts",
                columns: new[] { "city_id", "slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "amenities");

            migrationBuilder.DropTable(
                name: "city_categories");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.DropTable(
                name: "cities");
        }
    }
}
