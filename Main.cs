using System;

using Whetstone;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Parallel;

using System.Text;

using System.IO;

using System.Diagnostics;

namespace TextCharacteristicLearner
{
	class MainClass
	{
		public static int Main (string[] args)
		{

			bool runClassification = true;
			bool runDerivation = true;

			//TODO: USE THESE!
			string outDirectory = null;
			string inFile = null;

			int count = 100;

			int iterations = 1;

			string[] options = "-c;-d;-n;-i;-h".Split (';');
			string[] descriptions = "Run classification;Run classifier derivation;Number of instances to use (0 for all);Set the number of accuracy analysis iterations;Display this helpful message".Split (';');


			for(int i = 0; i < args.Length; i++){
				//Console.WriteLine (args[i]);
				switch(args[i]){
					case "-c": runClassification = true; break;
					case "-d": runDerivation = true; break;
					case "-n": 
						if(i == args.Length - 1 || !Int32.TryParse(args[++i], out count)){
							Console.WriteLine ("Please provide an integer argument following \"-n\".");
							return 1;
						}
						break;
					/*
					case "-o":
						if(i == args.Length - 1){
							Console.WriteLine ("Please provide a string argument following \"-o\".");
							return 0;
						}
					*/
					case "-i":
						if(i == args.Length - 1 || !Int32.TryParse(args[++i], out iterations)){
							Console.WriteLine ("Please provide an integer argument following \"-i\".");
							return 1;
						}
						break;
						
					case "--help":
					case "-h":
						Console.WriteLine (options.Zip (descriptions, (opt, desc) => opt + ": " + desc + ".").FoldToString ("Text Classification Suite\nUSAGE: [Input Dataset File] [Output Directory] [FLAGS]\n\n", "\n", "\n"));
						return 1;
					default:
						if(inFile == null){
							inFile = args[i];
						}
						else if(outDirectory == null){
							outDirectory = args[i];

							if(!outDirectory.EndsWith("/")){
								Console.WriteLine ("Warning: Please ensure output directory \"" + outDirectory + "\" is a directory.");
								outDirectory += "/";
							}
						}
						else{
							Console.WriteLine ("Failed to parse input \"" + args[i] + "\".  Try \"-h\" for help.");
							return 1;
						}
						break;
				}
			}

			if(inFile == null || outDirectory == null){
				Console.WriteLine ("WARNING: input filename and and output directory not provided.");
				Console.WriteLine ("Using defaults.");
			
				inFile = "../../res/shirishmed";
			}


			Stopwatch sw = new Stopwatch();
			sw.Start ();

			//basicClassifierTest();
			//testClassifiers();

			if(runClassification) runNewsClassification(inFile, outDirectory == null ? "../../out/news/classifications/" : outDirectory, count, iterations);
			if(runDerivation) runNewsClassifierDerivation(inFile, outDirectory == null ? "../../out/news/classifierderivation/" : outDirectory, count, iterations);

			//testNews ();
			//TestLatex ();
			//TestBrokenNormalizer();

			//TestNewDesign();
			//deriveOptimalClassifier();

			//testDatabase ();

			sw.Stop ();

			Console.WriteLine ("Elapsed Time: " + sw.Elapsed);

			//testClassifiers();
			return 0;
		}

		public static void TestNewDesign(){
			DiscreteSeriesDatabase<string> allData = LoadRegionsDatabase();

			Tuple<DiscreteSeriesDatabase<string>, DiscreteSeriesDatabase<string>> split = allData.SplitDatabase (.8);

			DiscreteSeriesDatabase<string> trainingData = split.Item1;
			DiscreteSeriesDatabase<string> testData = split.Item2;


			IFeatureSynthesizer<string> synth = new RegressorFeatureSynthesizerKmerFrequenciesVarK<string>("region", 8, 2, 100, 3);
			//IFeatureSynthesizer<string> synth = new RegressorFeatureSynthesizerKmerFrequencies<string>("region", 4, 10, 100, 3);
			//IFeatureSynthesizer<string> synth = new RegressorFeatureSynthesizerFrequencies<string>("region", 4, 10, 100);
			 
			synth.Train (trainingData);

			Console.WriteLine (synth.ToString ());
			synth.ScoreModel (testData, 2, "filename");
			Console.WriteLine(ClassifyDataSet (synth, testData, "filename")); //TODO may be good to use something unspecifiable in the file syntax such as "filename;"
		

			//Console.WriteLine (allData.DatabaseLatexString("Regional Spanish Database"));
		}

		/*
		public void TestNewClassifiers(){

		}
		*/

		public static void TestLatex ()
		{

			bool test = true;
			bool shorten = true;
			bool costarica = true;
			bool cuba = true;

			if(test){
				costarica = cuba = false;
			}

			DiscreteSeriesDatabase<string> allData = LoadRegionsDatabase (test, shorten, costarica, cuba);


			/*
			IFeatureSynthesizer<string> testSynth = new VarKmerFrequencyFeatureSynthesizer<string>("region", 3, 4, 50, 2.0, true);
			testSynth.Train (allData);

			Console.WriteLine (testSynth.GetFeatureSchema().FoldToString ());
			Console.WriteLine (testSynth.SynthesizeFeaturesSumToOne(new DiscreteEventSeries<string>(allData.data.First ().labels, allData.data.First ().Take (25).ToArray ())).FoldToString (d => d.ToString ("F3")));
			Console.ReadLine ();
			*/

			/*
			if(test){
				allData = allData.SplitDatabase (.25).Item1;
			}
			*/


			//TODO: Add length distribution for documents and each type.

			//Create a feature synthesizer

			//IFeatureSynthesizer<string> synth = new RegressorFeatureSynthesizerKmerFrequenciesVarK<string>("region", 8, 2, 100, 3); //Slowld way
			//IFeatureSynthesizer<string> synth = new VarKmerFrequencyFeatureSynthesizer<string>("region", 3, 4, 50, 2.0, true);

			//IEventSeriesProbabalisticClassifier<string> textClassifier // = TextClassifierFactory.TextClassifier ("region", new[]{"region", "type"});

			//string documentTitle, string author, int width, int height, string outFile, IEnumerable<Tuple<string, IEventSeriesProbabalisticClassifier<Ty>>> classifiers, string datasetTitle, DiscreteSeriesDatabase<Ty> dataset, string criterionByWhichToClassify
			//IEnumerable<Tuple<string, IEventSeriesProbabalisticClassifier<string>>> classifiers = TextClassifierFactory.RegionsTestClassifiers().ToArray ();
			IEnumerable<Tuple<string, IEventSeriesProbabalisticClassifier<string>>> classifiers = TextClassifierFactory.RegionsPerceptronTestClassifiers().ToArray ();

			IFeatureSynthesizer<string> synthesizer = new CompoundFeatureSynthesizer<string>(
				"region",
				new IFeatureSynthesizer<string>[]{
					new VarKmerFrequencyFeatureSynthesizerToRawFrequencies<string>("region", 2, 2, 16, .1, false),
					new LatinLanguageFeatureSynthesizer("region"),
					new VarKmerFrequencyFeatureSynthesizer<string>("region", 3, 4, 50, 2.0, false),
					new VarKmerFrequencyFeatureSynthesizer<string>("type", 3, 3, 50, 2.0, false)
				}
			);


			if(test){
				classifiers = classifiers.Take (2);
			}



			WriteupGenerator.ProduceClassifierComparisonWriteup<string>("Spanish Language Dialect Analysis", "Cyrus Cousins", 11, 16, "../../out/spanish/classification/", classifiers.ToArray (), "Spanish Language", allData, "region", test ? 1 : 4, analysisCriteria: new[]{"region", "type"}, synthesizer: synthesizer);

			/*
			if (classifier is SeriesFeatureSynthesizerToVectorProbabalisticClassifierEventSeriesProbabalisticClassifier<string>) {
				IFeatureSynthesizer<string> synthesizer = ((SeriesFeatureSynthesizerToVectorProbabalisticClassifierEventSeriesProbabalisticClassifier<string>)classifier).synthesizer;
				
				//doc.Append ("\\section{Feature Synthesizer Analysis}\n\n");
				//doc.Append (synthesizer.FeatureSynthesizerLatexString(allData));
			}
			*/


		}

		//NAME PROCESSING:
			
	    static string NameCase (string value)
		{
			char[] array = value.ToCharArray ();
			//First
			if (array.Length >= 1) {
				if (char.IsLower (array [0])) {
					array [0] = char.ToUpper (array [0]);
				}
			}
			//Rest
			for (int i = 1; i < array.Length; i++) {
				if (array [i - 1] == ' ') {
					if (char.IsLower (array [i])) {
						array [i] = char.ToUpper (array[i]);
					}
				}
				else if(char.IsUpper (array[i])){
					array[i] = char.ToLower (array[i]);
				}
			}
			return new string (array);
		}

		private static HashSet<String> invalidAuthors = new HashSet<string>("Porst Report;Posr Report;Post Reoprt;Post Repoert;Post Report;Post Repo-rt;POST REPORT;POST REPORT \'environmental Laws Adequate, Implementation Weak\';POST REPORT P\';POST REPORT, POST REPORT;Post Repot;Post Reprot;Post Rerport;Post Roport;Post Team;PR;Pr);(pr);PR, PR;RSS;;Rss.;(rss;(rss)".Split (';'));
		private static Dictionary<string, string> manualRenames = 
			"Priyakur Mandav:Priyankur Mandav;Milanmani Sharma:Milan Mani Sharma;Dr Sudhamshu K C:Dr Sudhamshu K.c.;Shrsisti_Shrestha;Shristi_Shrestha;Thomas_L._Friedman;Thomas_L_Friedman;William_Pfaff:William_Pfaf;Shandip_K C:Shandip_K.c.;Shandip_Kc:Shandip_K.c.;William_Pesek_Jr:Williar_Pesek_Jr.;William_Pesekjr:Williar_Pesek_Jr.;Prbhakar_Ghimire:Prabhakar_Ghimire;Himesh_Barjrachrya:Himesh_Bajracharya;Tapas_Barshimha_Thapa:Tapas_Barsimha_Thapa".Replace ("_", @" ").Split (";:".ToCharArray()).AdjacentPairs().ToDictionary(tup => tup.Item1, tup =>tup.Item2);
			//new Dictionary<string, string>();
		private static Dictionary<string, string> manualLocationRenames =
			"KATHMANDDU:KATHMANDU;KATHAMNDU:KATHMANDU".Split (";:".ToCharArray()).AdjacentPairs().ToDictionary(tup => tup.Item1, tup =>tup.Item2);
		private static HashSet<string> maleNames = new HashSet<string>(
			"ajaya;dhruva;dhruba;dipenra;krishna;lava;pierre;rishi;serge;shambhu;shashi;shiva;siddhi".Split (';')
		);
		private static HashSet<string> femaleNames = new HashSet<string>(
			"anjali;ann;anne;barbara;catherine;ellen;indu;jamie;jasmine;neeyati;sheryl;sue;karen;kathy;betty;ellen;gitanjali;ishwori;jan;jennifer;jill;laurie;manjushree;maureen;neeti;neeyati;sheryl;shristi;;N;bijaya;ah;chandra;jaya;hira;indra;jos;merrick;mvemba;nitya;padma;prakriti".Split (';')
		);
		private static HashSet<string> neutralNames = new HashSet<string>(
			"ang;susan".Split (';')
		);
		private static HashSet<string> titles = new HashSet<string>(
			"dr;doctor;prof;professor".Split (';')
		);

		public static DiscreteSeriesDatabase<string> getNewsDataset (string fileName, int count = 0)
		{
			DiscreteSeriesDatabase<string> data = new DiscreteSeriesDatabase<string> ();

			using (StreamReader keyfile = File.OpenText(fileName + "key")) {
				if(count > 0){
					keyfile.BaseStream.Seek (-107 * count, System.IO.SeekOrigin.End); //avg line is ~81 characters.
					keyfile.ReadLine ();
				}
//				for(int i = 0; i < 8000; i++) keyfile.ReadLine ();
				data.LoadTextDatabase (fileName + "/", keyfile, DatabaseLoader.ProcessEnglishText, 1);
			}

			//Do some processing on the database
			foreach (DiscreteEventSeries<string> item in data.data) {
				string author = AsciiOnly (item.labels ["author"], false).RegexReplace (@"_+", @" ").RegexReplace (@"(?:[<])|(?:^[ ,])|(?:$)|(?:\')|(?:\\)", "").RegexReplace (@"([#$&])", @"\$1");
				author = manualRenames.GetWithDefault (author, author);

				if (author.StartsWith (@" ")) { //TODO: Why is this not caught by the regex?
					author = author.Substring (1);
				}
				if (invalidAuthors.Contains (author)) {
					//Console.WriteLine ("REMOVED " + author);
					item.labels.Remove ("author");
				} else {
					item.labels ["author"] = NameCase(author); //Put the formatting done above back into db

					string[] authSplit = author.Split(' ');
					string firstName = authSplit[0].ToLower ();
					if(titles.Contains(firstName) && authSplit.Length > 1){
						if(authSplit.Length == 2){
							//Just a last name.
							firstName = "a"; //Will be marked neutral.
						}
						else{
							firstName = authSplit[1];
						}
					}

					if(neutralNames.Contains(firstName) || firstName.Length == 1){
						//Gender unknown
					}
					else if(maleNames.Contains (firstName) || firstName.EndsWith ("ndra")){
						item.labels["gender"] = "male";
					}
					else if(firstName[firstName.Length - 1] == 'a' || firstName.EndsWith ("ee") || femaleNames.Contains(firstName)){
						item.labels["gender"] = "female";
					}
					else if("eiou".Contains (firstName[firstName.Length - 1])){
						//Gender unknown (suspected female)
					}
					else if(firstName.Length > 1){
						item.labels["gender"] = "male";
					}
				}

				item.labels ["filename"] = item.labels ["filename"].Replace ("_", " ").RegexReplace ("([#$&])", "\\$1");
				if (item.labels.ContainsKey ("location")){
					item.labels ["location"] = item.labels ["location"].Replace ("_", " ").RegexReplace ("([#$&])", "\\$1");
					item.labels ["location"] = manualLocationRenames.GetWithDefault (item.labels["location"], item.labels["location"]);
					item.labels ["location"] = NameCase (item.labels ["location"]);
				}
			}

			return data;
		}

		public static void runNewsClassification(string inFile, string outDirectory, int count, int iterations){
			
			DiscreteSeriesDatabase<string> data = getNewsDataset (inFile, count);

			//Create the classifier
			/*
			IEventSeriesProbabalisticClassifier<string> classifier = new SeriesFeatureSynthesizerToVectorProbabalisticClassifierEventSeriesProbabalisticClassifier<string>(
				new VarKmerFrequencyFeatureSynthesizer<string>("author", 3, 2, 60, 0.1, false),
				new NullProbabalisticClassifier()
			);
			*/
			
			IEventSeriesProbabalisticClassifier<string> classifier = new SeriesFeatureSynthesizerToVectorProbabalisticClassifierEventSeriesProbabalisticClassifier<string>(
				new VarKmerFrequencyFeatureSynthesizer<string>("author", 3, 2, 50, 0.6, false),
				new PerceptronCloud(16.0, PerceptronTrainingMode.TRAIN_ALL_DATA, PerceptronClassificationMode.USE_NEGATIVES | PerceptronClassificationMode.USE_SCORES, 1.5, false)
			);

			//string documentTitle, string author, int width, int height, string outFile, IEventSeriesProbabalisticClassifier<Ty> classifier, DiscreteEventSeries<Ty> dataset, string datasetTitle, string criterionByWhichToClassify
			WriteupGenerator.ProduceClassificationReport<string>("Analysis and Classification of " + data.data.Count + " Ekantipur Articles", "Cyrus Cousins with Shirish Pokharel", 20, 20, outDirectory, classifier, "characteristic kmer classifier", data, "News", "author", iterations);

		}
		public static void runNewsClassifierDerivation (string inFile, string outDirectory, int count, int iterations)
		{

			//Load the database:
			DiscreteSeriesDatabase<string> data = getNewsDataset (inFile, count);
			//data = data.SplitDatabase (.1).Item1;


			IEnumerable<Tuple<string, IEventSeriesProbabalisticClassifier<string>>> classifiers = TextClassifierFactory.NewsTestClassifiers().Concat(TextClassifierFactory.NewsTestAdvancedClassifiers().Skip (1));
			IFeatureSynthesizer<string> synth = new CompoundFeatureSynthesizer<string>("author", new IFeatureSynthesizer<string>[]{
				new VarKmerFrequencyFeatureSynthesizer<string>("author", 3, 2, 60, 0.7, false),
				new VarKmerFrequencyFeatureSynthesizer<string>("location", 3, 3, 50, 1, false),
				new VarKmerFrequencyFeatureSynthesizer<string>("gender", 3, 8, 50, 10, false),
				new DateValueFeatureSynthesizer("date"),
				new LatinLanguageFeatureSynthesizer("author")
			});
			WriteupGenerator.ProduceClassifierComparisonWriteup<string>("Classifier Comparison Analysis on Ekantipur News Articles", "Cyrus Cousins with Shirish Pokharel", 20, 20, outDirectory, classifiers.ToArray (), "News", data, "author", iterations, new[]{"author", "location", "date", "gender"}, synth);
		}


		public static string AsciiOnly(string input, bool includeExtendedAscii)
		{
		    int upperLimit = includeExtendedAscii ? 255 : 127;
		    char[] asciiChars = input.Where(c => (int)c <= upperLimit).ToArray();
		    return new string(asciiChars);
		}



		
		public static IFeatureSynthesizer<string> deriveOptimalClassifier(){

			//Load databases
			DiscreteSeriesDatabase<string> allData = LoadRegionsDatabase();

			Tuple<DiscreteSeriesDatabase<string>, DiscreteSeriesDatabase<string>> split = allData.SplitDatabase (.8);

			DiscreteSeriesDatabase<string> trainingData = split.Item1;
			DiscreteSeriesDatabase<string> testData = split.Item2;

			string cat = "region";

			double optimalScore = 0;
			IFeatureSynthesizer<string> optimalClassifier = null;
			string optimalInfoStr = null;

			//Preliminary scan
			
			int[] ks = new int[]{2, 3, 4};
			//int[] minCutoffs = new int[]{5, 10, 20};
			int[] minCutoffs = new int[]{10};
			int[] kmerCounts = new int[]{10, 25, 50, 100};
			int[] smoothingAmounts = new int[]{1, 5, 10};

			string[] colNames = "k minCutoff kmerCount smoothingAmount score".Split (' ');

			Console.WriteLine (colNames.FoldToString ("", "", ","));

			foreach(int k in ks){
				foreach(int minCutoff in minCutoffs){
					foreach(int kmerCount in kmerCounts){
						foreach(int smoothingAmount in smoothingAmounts){

							IFeatureSynthesizer<string> classifier = new RegressorFeatureSynthesizerKmerFrequenciesVarK<string>(cat, minCutoff, smoothingAmount, kmerCount, k);
							classifier.Train (trainingData);

							double score = classifier.ScoreModel (testData);

							string infoStr = new double[]{k, minCutoff, kmerCount, smoothingAmount, score}.FoldToString ("", "", ",");

							Console.WriteLine (infoStr);
							if(score > optimalScore){
								optimalScore = score;
								optimalClassifier = classifier;
								optimalInfoStr = infoStr;
							}
						}
					}
				}
			}

			Console.WriteLine ("Optimal Classifier:");
			Console.WriteLine (optimalInfoStr);
			Console.WriteLine (optimalClassifier);

			return optimalClassifier;

		}

		public static DiscreteSeriesDatabase<string> LoadRegionsDatabase (bool test = false, bool shorten = false, bool costarica = true, bool cuba = true)
		{
			//Load training data and create classifier.

			string directory = "../../res/regiones/";

			string[] regions = "españa argentina méxico colombia".Split (' ');

			string file = "";

			if(costarica){
				regions = "costarica".Cons (regions).ToArray ();
			}
			if(cuba){
				regions = "cuba".Cons (regions).ToArray ();
			}

			//string[] prefixes = new[]{"", "literatura", "historia", "lengua"};
			//file += prefixes.Select (prefix => regions.FoldToString ((sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "news" + " " + prefix + val, "", "", "\n")).FoldToString ("", "", "\n");

			file += regions.Aggregate ("", (sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "news" + " " + val + "\n");
			file += regions.Aggregate ("", (sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "wiki" + " " + "literatura" + val + "\n");
			file += regions.Aggregate ("", (sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "wiki" + " " + "historia" + val + "\n");
			file += regions.Aggregate ("", (sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "wiki" + " " + "lengua" + val + "\n");
			file += regions.Aggregate ("", (sum, val) => sum + "region" + ":" + val + ";" + "type" + ":" + "receta" + " " + "recetas" + val + "\n");

			if (!test) {
				{
					string[] literatureRegions = "costarica costarica españa españa españa argentina argentina argentina argentina argentina argentina españa españa españa españa méxico méxico méxico méxico méxico méxico méxico colombia colombia colombia colombia colombia".Split (' ');
					string[] literatureNames = "leyendascr elisadelmar juanvaleraavuelaplumaespaña juanvaleraloscordobesesespaña marianela historiauniversal lamuerte buenosaires derroterosyviages fundaciondelaciudad laargentina mosenmillan historiadejudios viajosporespaña recuerdosybellezas leyendasmayas nahuatl laberinto comoaguaparachocolate mitoshorroresmexicanos leyendasmexicanas mitosurbanesmexicanos lamultituderrante viajoscolombianos leyendasurbanascolombianas mitoscolombianos mitoscolombianos2".Split (' ');

					IEnumerable<string> classesStrings = literatureRegions.Select (r => "region:" + r + ";" + "type:" + "literature");

					file += classesStrings.Zip (literatureNames, (thisClasses, thisPath) => thisClasses + " " + thisPath).Aggregate (new StringBuilder (), (sum, val) => sum.Append (val).Append ("\n"));
				}

				{
					string[] names = (
					"salud antologia9 escorpionescr teca vacunoscr lanación universidadcr recetascostarica2 recetascostarica3 crcrawl presidentecostarica gobiernocostarica " +
						"arqueologiamaya poesiamexicana catolicismosocial unam mxcrawl cocrawl cocrawl2 desplazadoscolombianos mexicocnn méxicolgbt méxicogob historiaazteca historiaazteca2 " +
						"ordenamientoterretorrial competitividad ministerio"
				).Split (' ');
					string[] tags = (
					"region:costarica region:costarica region:costarica region:costarica region:costarica;type:paper region:costarica;type:news region:costarica region:costarica;type:receta region:costarica;type:receta region:costarica;type:website region:costarica;type:wiki region:costarica;type:wiki " +
						"region:méxico region:méxico;type:paper region:méxico;type:paper region:méxico;type:paper region:méxico;type:website region:colombia;type:website region:colombia;type:website region:colombia;type:wiki region:méxico;type:news region:méxico;type:brochure region:méxico;type:website region:méxico region:méxico " +
						"region:colombia region:colombia region:colombia"
				).Split (' ');

					file += tags.Zip (names, (tag, name) => tag + " " + name).FoldToString ("", "\n", "\n");
				}
			}

			if(cuba){
				file += "region:cuba;type:wiki cubaisla\n";
				file += "region:cuba;type:receta recetascuba2\n";
				file += "region:cuba;type:receta recetascuba3\n";
				file += "region:cuba;type:literatura lahistoriame\n";
				file += "region:cuba;type:literatura elencuentro\n";
			}

			Console.WriteLine ("Regions Database:");
			Console.WriteLine(file);

			TextReader reader = new StringReader(file);

			DiscreteSeriesDatabase<string> d = new DiscreteSeriesDatabase<string> ();
			d.LoadTextDatabase (directory, reader, DatabaseLoader.ProcessSpanishText, 3);

			if(shorten){
				d = new DiscreteSeriesDatabase<string>(d.Select (item => new DiscreteEventSeries<string>(item.labels, item.data.Take (750).ToArray ())));
			}

			return d;
		}



		//CLASSIFICATION:
		public static string ClassifyDataSet<Ty>(IFeatureSynthesizer<Ty> synth, DiscreteSeriesDatabase<Ty> db, string nameField){
			return db.data.AsParallel().Select (item => ClassifyItem(synth, item, nameField)).FoldToString ();
		}

		public static string ClassifyItem<Ty>(IFeatureSynthesizer<Ty> synth, DiscreteEventSeries<Ty> item, string nameField){

			double[] scores = synth.SynthesizeFeaturesSumToOne(item);

			double max = scores.Max ();
			//TODO don't report ambiguous cases.
			return (item.labels[nameField] + ": " + synth.SynthesizeLabelFeature(item) + "" +
				"(" + max + " confidence)");
		}




		public static void TestBrokenNormalizer(){
			ZScoreNormalizerClassifierWrapper normalizer = new ZScoreNormalizerClassifierWrapper(new NullProbabalisticClassifier());

			double[][] data = new double[][]{
				new double[] {-1, 100, 100, 0},
				new double[] {0, 0, 120, 0},
				new double[] {1, -100, 80, 0}
			};

			IEnumerable<LabeledInstance> tdata = 
				data.Select ((vals, index) => new LabeledInstance(index.ToString(), vals));

			normalizer.Train (tdata);

			Console.WriteLine ("Normalizer: " + normalizer);

			double[] test = {10, 10, 100, 7};

			Console.WriteLine ("Z(" + test.FoldToString () + ") = " + normalizer.applyNormalization(test).FoldToString ());
		}
	}
}
