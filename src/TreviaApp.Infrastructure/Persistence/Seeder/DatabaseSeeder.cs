namespace TreviaApp.Infrastructure.Persistence.Seeder;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TreviaApp.Domain.Exercises;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public class DatabaseSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AdminSeedOptions _adminOptions;
    private readonly ApplicationDbContext _db;

    public DatabaseSeeder(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOptions<AdminSeedOptions> adminOptions, ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminOptions = adminOptions.Value;
        _db = db;
    }

    public async Task SeedAllAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedAdminAsync(ct);
        await SeedInitialExercisesAsync(ct);
        await GamificationSeeder.SeedAsync(_db, ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        var roles = new (string Name, string Desc)[]
        {
            (AppRoles.Administrator, "Acesso total ao sistema"),
            (AppRoles.Student, "Aluno que realiza treinos"),
            (AppRoles.Trainer, "Professor que cria fichas e acompanha alunos"),
            (AppRoles.GymManager, "Gestor de academia")
        };

        foreach (var (name, desc) in roles)
        {
            if (!await _roleManager.RoleExistsAsync(name))
                await _roleManager.CreateAsync(new AppRole(name) { Description = desc });
        }
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_adminOptions.Email) || string.IsNullOrWhiteSpace(_adminOptions.Password))
            return;

        if (await _userManager.FindByEmailAsync(_adminOptions.Email) != null)
            return;

        var admin = new AppUser
        {
            UserName = _adminOptions.Email,
            Email = _adminOptions.Email,
            FirstName = "Administrador",
            LastName = "Sistema",
            DisplayName = "Admin",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, _adminOptions.Password);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(admin, AppRoles.Administrator);
    }

    public async Task SeedInitialExercisesAsync(CancellationToken ct = default)
    {
        var adminUser = await _userManager.FindByEmailAsync(_adminOptions.Email);
        if (adminUser == null) return;

        if (await _db.Exercises.AsNoTracking().AnyAsync(ct)) return;

        var listExercises = BuildInitialExercises(adminUser.Id);
        _db.Exercises.AddRange(listExercises);
        await _db.SaveChangesAsync(ct);
    }

    private static List<Exercise> BuildInitialExercises(Guid adminId)
    {
        var list = new List<Exercise>();
        var now = DateTimeOffset.UtcNow;

        void Add(string name, string slug, DifficultyLevel diff, MeasurementType measure,
                 string instructions, string? tips, string? shortDesc, string? tags,
                 (Muscle Muscle, MuscleRole Role)[] muscles, Equipment[] equipments,
                 ExerciseModality modality = ExerciseModality.WeightTraining)
        {
            var ex = new Exercise(
                createdByUserId: adminId,
                name: name,
                slug: slug,
                environment: TrainingEnvironment.Gym,
                modality: modality,
                difficultyLevel: diff,
                measurementType: measure,
                instructions: instructions,
                shortDescription: shortDesc,
                tips: tips,
                tags: tags,
                visibility: Visibility.Public);

            ex.SubmitForApproval();
            ex.Approve(adminId);

            foreach (var (muscle, role) in muscles)
                ex.AddMuscle(muscle, role);

            foreach (var eq in equipments)
                ex.AddEquipment(eq);

            list.Add(ex);
        }

        Add("Agachamento Livre", "agachamento-livre", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Posicione a barra no trapézio, pés na largura dos ombros. Flexione os joelhos e quadril, agachando até coxas paralelas ao solo. Suba mantendo a coluna ereta e core ativado.",
            "Mantenha os joelhos alinhados com os pés. Evite que os joelhos cavem para dentro.",
            "Exercício composto para membros inferiores.", "pernas,quadriceps,gluteos",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary),
                (Muscle.ErectorSpinae, MuscleRole.Stabilizer)
            },
            new[] { Equipment.Barbell });

        Add("Agachamento Búlgaro", "agachamento-bulgaro", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Coloque a ponta do pé traseiro apoiado em um banco. Agache com a perna dianteira até 90 graus. O joelho traseiro quase toca o chão. Suba empurrando com o calcanhar dianteiro.",
            "Mantenha o tronco ligeiramente inclinado para frente. Apoie-se no calcanhar da perna da frente.",
            "Agachamento unilateral com foco em força e equilíbrio.", "unilateral,pernas",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Leg Press 45°", "leg-press-45", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se na máquina, pés na plataforma na largura dos ombros. Deslize o assento flexionando os joelhos até 90°. Estenda as pernas sem travar os joelhos. Retorne controlado.",
            "Não deixe os joelhos ultrapassarem a linha dos pés. Suba com força pelos calcanhares.",
            "Exercício em máquina para quadríceps e glúteos.", "maquina,pernas",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary)
            },
            new[] { Equipment.Machine });

        Add("Cadeira Extensora", "cadeira-extensora", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se na máquina com apoio na parte anterior das canelas. Estenda os joelhos completamente contra a carga. Retorne controlado, sem soltar o peso bruscamente.",
            "Evite balançar o tronco. Faça o movimento lentamente e com controle.",
            "Isolamento para quadríceps.", "isolamento,quadriceps",
            new[] { (Muscle.Quads, MuscleRole.Primary) },
            new[] { Equipment.Machine });

        Add("Cadeira Flexora", "cadeira-flexora", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se na máquina com apoio na parte posterior das canelas. Flexione os joelhos trazendo os calcanhares em direção aos glúteos. Estenda controlado.",
            "Contraia os isquiotibiais no final do movimento por 1 segundo.",
            "Isolamento para isquiotibiais sentado.", "isquiotibiais,posterior",
            new[] { (Muscle.Hamstrings, MuscleRole.Primary) },
            new[] { Equipment.Machine });

        Add("Mesa Flexora", "mesa-flexora", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Deite-se de bruços na máquina, canelas apoiadas no rolo. Flexione os joelhos, trazendo os calcanhares para cima. Estenda as pernas lentamente.",
            "Mantenha o quadril encaixado para não levantar a pelve do banco.",
            "Isolamento para isquiotibiais deitado.", "isquiotibiais,posterior",
            new[] { (Muscle.Hamstrings, MuscleRole.Primary) },
            new[] { Equipment.Machine });

        Add("Stiff", "stiff", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Em pé, barra na frente das coxas. Flexione o quadril, projetando o bumbum para trás. Desça a barra rente às pernas até sentir estiramento nos ísquios. Suba contraindo glúteos.",
            "Mantenha os joelhos semiflexos, coluna neutra. O movimento é no quadril.",
            "Levantamento terra romeno para isquiotibiais e glúteos.", "romeno,terra",
            new[]
            {
                (Muscle.Hamstrings, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.ErectorSpinae, MuscleRole.Stabilizer)
            },
            new[] { Equipment.Barbell });

        Add("Levantamento Terra Convencional", "levantamento-terra", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Pés na largura do quadril, barra sobre os tornozelos. Agache, pegue a barra pronada. Estenda quadril e joelhos para ficar em pé. Desça a barra controlada ao chão.",
            "Mantenha coluna neutra durante todo movimento. Core bem ativado.",
            "Exercício composto de corpo inteiro.", "composto,terra,total",
            new[]
            {
                (Muscle.ErectorSpinae, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Primary),
                (Muscle.Quads, MuscleRole.Secondary),
                (Muscle.Back, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell });

        Add("Panturrilha em Pé na Máquina", "panturrilha-pe-maquina", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Coloque os ombros sob as almofadas. Apoie as pontas dos pés na plataforma, calcanhares para baixo. Levante os calcanhares o máximo possível. Baixe devagar, sentindo o alongamento.",
            "Faça pausa de 1-2s no topo e no fundo do movimento.",
            "Treino de panturrilhas em pé.", "gastrocnemio, panturrilha",
            new[] { (Muscle.Calves, MuscleRole.Primary) },
            new[] { Equipment.Machine });

        Add("Panturrilha Sentado", "panturrilha-sentado", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se na máquina, joelhos sob a almofada. Pontas dos pés na plataforma. Eleve os calcanhares o máximo, contraia. Baixe lentamente para alongar o sóleo.",
            "Foco no músculo sóleo por causa da posição sentada.",
            "Panturrilha sentada foca no sóleo.", "soleo,panturrilha",
            new[]
            {
                (Muscle.Soleus, MuscleRole.Primary),
                (Muscle.Calves, MuscleRole.Secondary)
            },
            new[] { Equipment.Machine });

        Add("Supino Reto com Barra", "supino-reto-barra", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Deite-se no banco plano. Pés firmes no chão. Pegue a barra um pouco mais larga que os ombros. Desça a barra até o meio do peito. Empurre a barra para cima estendendo os braços.",
            "Retraia as escápulas antes de descer. Core bem contraído.",
            "Exercício rei para peitoral.", "peito,barra",
            new[]
            {
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Secondary),
                (Muscle.DeltoidsAnterior, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell, Equipment.Bench });

        Add("Supino Inclinado com Halteres", "supino-inclinado-halteres", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Banco inclinado a 30-45 graus. Segure halteres na altura do peito. Empurre para cima até braços quase estendidos. Baixe controlado, abrindo os cotovelos para fora.",
            "Não trave os cotovelos no topo para manter tensão no peito.",
            "Foco na porção superior do peitoral.", "peito-superior",
            new[]
            {
                (Muscle.PectoralisMajorClavicular, MuscleRole.Primary),
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.DeltoidsAnterior, MuscleRole.Secondary),
                (Muscle.Triceps, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Supino Declinado", "supino-declinado", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Banco declinado em 15-30 graus. Segure a barra ou halteres. Desça a carga na parte inferior do peito. Suba empurrando para cima.",
            "Respire fundo antes de descer. Segure a respiração na subida.",
            "Foco na porção inferior do peitoral.", "peito-inferior",
            new[]
            {
                (Muscle.PectoralisMajorSternal, MuscleRole.Primary),
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell, Equipment.Bench });

        Add("Crucifixo com Halteres", "crucifixo-halteres", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Deite-se no banco plano. Halteres sobre o peito, palmas viradas uma para a outra. Abra os braços em arco, sentindo o peito se alongar. Feche os braços voltando à posição inicial.",
            "Mantenha os cotovelos levemente flexionados. Use carga leve a moderada.",
            "Exercício de alongamento e contração do peitoral.", "alongamento,peito",
            new[]
            {
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.DeltoidsAnterior, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Crossover (Cabos)", "crossover", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Ajuste as polias em altura média ou superior. Pegue as manoplas. Dê um passo à frente, tronco inclinado. Puxe as manoplas em direção ao centro, cruzando as mãos à frente do corpo. Volte controlado.",
            "Contraia o peito no ponto final do movimento por 1 segundo.",
            "Peito com cabos — tensão constante.", "cabos,peito",
            new[]
            {
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.DeltoidsAnterior, MuscleRole.Secondary)
            },
            new[] { Equipment.Cable, Equipment.CableCrossover });

        Add("Flexão de Braços (Push-up)", "flexao-bracos", DifficultyLevel.Beginner, MeasurementType.Bodyweight,
            "Apoie as mãos no chão, ligeiramente mais largas que os ombros. Corpo alinhado. Desça flexionando os cotovelos até peito quase tocar o chão. Empurre para subir.",
            "Core contraído, bumbum alinhado. Se for difícil, inicie de joelhos.",
            "Clássico exercício de peso corporal.", "pushup,calistenia,peito",
            new[]
            {
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Primary),
                (Muscle.DeltoidsAnterior, MuscleRole.Secondary),
                (Muscle.Abs, MuscleRole.Stabilizer)
            },
            new[] { Equipment.Bodyweight },
            ExerciseModality.Bodyweight);

        Add("Barra Fixa (Pull-up)", "barra-fixa", DifficultyLevel.Intermediate, MeasurementType.Bodyweight,
            "Pendure-se na barra com pegada pronada, mãos mais largas que ombros. Puxe o corpo para cima até que o queixo ultrapasse a barra. Desça controlado até braços estendidos.",
            "Não balançe o corpo. Puxe com os cotovelos.",
            "Rei dos exercícios para costas.", "costas,dorsal,calistenia",
            new[]
            {
                (Muscle.LatissimusDorsi, MuscleRole.Primary),
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.Biceps, MuscleRole.Secondary),
                (Muscle.Forearms, MuscleRole.Secondary)
            },
            new[] { Equipment.PullUpBar },
            ExerciseModality.Bodyweight);

        Add("Puxada Alta Frente", "puxada-alta-frente", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se na máquina, coxas travadas. Pegue a barra larga. Puxe a barra na altura do peito, encostando na porção superior. Solte lentamente estendendo os braços.",
            "Puxe com os cotovelos apontando para baixo/lados. Não arqueie muito as costas.",
            "Lat pulldown para dorsal.", "maquina,costas,dorsal",
            new[]
            {
                (Muscle.LatissimusDorsi, MuscleRole.Primary),
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.Biceps, MuscleRole.Secondary)
            },
            new[] { Equipment.Machine });

        Add("Remada Curvada com Barra", "remada-curvada-barra", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Barra no chão ou em racks. Incline o tronco a 45°, joelhos semiflexos. Pegue a barra com pegada pronada. Puxe a barra em direção à cintura. Abaixe controlado.",
            "Puxe com os cotovelos para trás, não com os braços. Contraia as escápulas.",
            "Remada para espessura da costas.", "costas,barra,remada",
            new[]
            {
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.LatissimusDorsi, MuscleRole.Primary),
                (Muscle.Biceps, MuscleRole.Secondary),
                (Muscle.Rhomboids, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell });

        Add("Remada Baixa (Cabo, Triângulo)", "remada-baixa-cabo", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se no cabo, pés apoiados. Tronco ereto, pegue o triângulo. Puxe o cabo em direção ao abdômen, puxando as escápulas para trás. Volte devagar estendendo os braços.",
            "Não arqueie a coluna. Puxe com as costas, não com os braços.",
            "Remada baixa com cabo para meio das costas.", "cabo,costas,meio-costas",
            new[]
            {
                (Muscle.Rhomboids, MuscleRole.Primary),
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.LatissimusDorsi, MuscleRole.Secondary),
                (Muscle.Biceps, MuscleRole.Secondary)
            },
            new[] { Equipment.Cable });

        Add("Remada Unilateral com Halter", "remada-unilateral-halter", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Apoie um joelho e mão no banco, outro pé no chão. Pegue o halter com a mão livre. Puxe o halter até a cintura, puxando o cotovelo para cima. Baixe devagar.",
            "Mantenha a coluna neutra. Contraia as escápulas no topo.",
            "Remada unilateral para costas.", "costas,unilateral,halter",
            new[]
            {
                (Muscle.LatissimusDorsi, MuscleRole.Primary),
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.Rhomboids, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Desenvolvimento Militar com Barra", "desenvolvimento-militar-barra", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Em pé, barra na altura do queixo, pegada pronada. Empurre a barra para cima estendendo os braços. Baixe devagar até a altura inicial.",
            "Core bem contraído para não arquear a coluna lombar.",
            "Desenvolvimento para ombros com barra.", "ombros,barra",
            new[]
            {
                (Muscle.DeltoidsAnterior, MuscleRole.Primary),
                (Muscle.Shoulders, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Secondary),
                (Muscle.Trapezius, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell });

        Add("Desenvolvimento com Halteres Sentado", "desenvolvimento-halteres-sentado", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sente-se no banco com encosto reto. Halteres na altura dos ombros. Empurre ambos para cima até os braços quase totalmente estendidos. Baixe controlado.",
            "Apoie bem a coluna no encosto. Não trave os cotovelos no topo.",
            "Desenvolvimento sentado para ombros.", "ombros,halteres,sentado",
            new[]
            {
                (Muscle.Shoulders, MuscleRole.Primary),
                (Muscle.DeltoidsAnterior, MuscleRole.Primary),
                (Muscle.DeltoidsLateral, MuscleRole.Secondary),
                (Muscle.Triceps, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Elevação Lateral com Halteres", "elevacao-lateral-halteres", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Em pé, halteres ao lado do corpo com palmas para dentro. Levante os braços para os lados (como um 'T') até a altura dos ombros. Baixe lentamente.",
            "Use cargas leves. Contraia o deltóide lateral no topo.",
            "Isolamento para deltóide lateral.", "ombros,lateral",
            new[] { (Muscle.DeltoidsLateral, MuscleRole.Primary) },
            new[] { Equipment.Dumbbell });

        Add("Elevação Frontal com Halteres", "elevacao-frontal-halteres", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Em pé, halteres à frente das coxas. Levante um braço de cada vez (ou ambos) para frente até a altura dos ombros. Desça devagar.",
            "Mantenha o braço quase estendido (cotovelo levemente flexionado).",
            "Isolamento para deltóide anterior.", "ombros,frontal",
            new[] { (Muscle.DeltoidsAnterior, MuscleRole.Primary) },
            new[] { Equipment.Dumbbell });

        Add("Crucifixo Invertido", "crucifixo-invertido", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Inclinado com o tronco paralelo ao chão (sentado ou em pé), halteres pendentes. Abra os braços para os lados, apertando as escápulas. Baixe devagar.",
            "Movimento pequeno, focado no deltóide posterior e romboides.",
            "Foco em deltóide posterior e romboides.", "ombros-posterior,romboides",
            new[]
            {
                (Muscle.DeltoidsPosterior, MuscleRole.Primary),
                (Muscle.Rhomboids, MuscleRole.Primary)
            },
            new[] { Equipment.Dumbbell, Equipment.Bench });

        Add("Rosca Direta com Barra", "rosca-direta-barra", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Em pé, barra com pegada supinada. Mantenha os cotovelos colados ao tronco. Flexione os cotovelos, levando a barra até a altura do peito. Desça controlado.",
            "Não use impulso do corpo. Mantenha os cotovelos fixos.",
            "Clássico exercício para bíceps.", "biceps,barra",
            new[]
            {
                (Muscle.Biceps, MuscleRole.Primary),
                (Muscle.Brachialis, MuscleRole.Secondary),
                (Muscle.Forearms, MuscleRole.Secondary)
            },
            new[] { Equipment.Barbell });

        Add("Rosca Alternada com Halteres", "rosca-alternada-halteres", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Em pé, halteres ao lado do corpo. Flexione um cotovelo, subindo o halter até o ombro (supinado). Desça e faça o outro lado, alternando.",
            "Gire a palma da mão ao subir (martelo para supinação).",
            "Rosca alternada para bíceps bilateral.", "biceps,alternada",
            new[]
            {
                (Muscle.Biceps, MuscleRole.Primary),
                (Muscle.Brachialis, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell });

        Add("Rosca Martelo", "rosca-martelo", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Em pé, halteres com pegada neutra (palmas viradas uma para a outra). Flexione ambos os cotovelos, sem girar as mãos. Suba e desça controlado.",
            "Mantenha os cotovelos colados ao tronco. O foco é no braquialis.",
            "Foco em braquialis e braquiorradial.", "braquialis,martelo",
            new[]
            {
                (Muscle.Brachialis, MuscleRole.Primary),
                (Muscle.Biceps, MuscleRole.Secondary),
                (Muscle.Brachioradialis, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell });

        Add("Tríceps Pulley (Cabo, Barra Reta)", "triceps-pulley", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Fique em pé, colado à polia alta. Cotovelos colados no tronco, pegue a barra reta. Estenda os cotovelos empurrando a barra para baixo. Volte flexionando devagar.",
            "Mantenha os cotovelos fixos. Não deixe a carga subir com força.",
            "Exercício clássico para tríceps.", "triceps,cabo",
            new[]
            {
                (Muscle.TricepsLongHead, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Primary),
                (Muscle.TricepsLateralHead, MuscleRole.Secondary)
            },
            new[] { Equipment.Cable });

        Add("Tríceps Testa com Barra W", "triceps-testa-barra-w", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Deite-se no banco plano, barra W acima da testa. Cotovelos fixos apontando para cima. Flexione os cotovelos, baixando a barra em direção à testa. Estenda devagar.",
            "Não mova os cotovelos. Mantenha-os perpendicular ao chão.",
            "Tríceps testa — foco na cabeça longa.", "triceps,testa",
            new[]
            {
                (Muscle.TricepsLongHead, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Primary)
            },
            new[] { Equipment.Barbell, Equipment.Bench });

        Add("Tríceps Francês com Halter", "triceps-frances-halter", DifficultyLevel.Beginner, MeasurementType.WeightAndRepetitions,
            "Sentado ou em pé, segure 1 halter com ambas as mãos acima da cabeça. Cotovelos fixos, flexione e abaixe o halter atrás da cabeça. Estenda os braços para cima.",
            "Mantenha os cotovelos próximos à cabeça e apontando para cima.",
            "Tríceps francês unilateral ou bilateral.", "triceps,frances",
            new[]
            {
                (Muscle.TricepsLongHead, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Primary)
            },
            new[] { Equipment.Dumbbell });

        Add("Abdominal Infra na Barra Fixa (Elevação de Pernas)", "abdominal-infra-barra-fixa", DifficultyLevel.Intermediate, MeasurementType.Bodyweight,
            "Pendure-se na barra, braços estendidos. Core contraído, eleve as pernas (retas ou flexionadas) até a altura do quadril ou superior. Abaixe controlado.",
            "Não use impulso. Se for difícil, flexione os joelhos.",
            "Core inferior focado — abdominal infra.", "core,infra,barra-fixa",
            new[]
            {
                (Muscle.Abs, MuscleRole.Primary),
                (Muscle.HipFlexors, MuscleRole.Secondary)
            },
            new[] { Equipment.PullUpBar },
            ExerciseModality.Bodyweight);

        Add("Prancha Isométrica", "prancha-isometrica", DifficultyLevel.Beginner, MeasurementType.Time,
            "Apoie os antebraços e pontas dos pés no chão. Corpo em linha reta da cabeça aos pés. Core bem contraído. Mantenha a posição pelo tempo desejado.",
            "Não deixe o bumbum subir nem cair. Alinhe cabeça com coluna.",
            "Isometria clássica para fortalecimento do core.", "core,isometria",
            new[]
            {
                (Muscle.Abs, MuscleRole.Primary),
                (Muscle.TransversusAbdominis, MuscleRole.Primary),
                (Muscle.ErectorSpinae, MuscleRole.Stabilizer)
            },
            new[] { Equipment.Bodyweight, Equipment.Mat },
            ExerciseModality.Bodyweight);

        Add("Afundo (Lunge) Andando", "afundo-andando", DifficultyLevel.Intermediate, MeasurementType.WeightAndRepetitions,
            "Em pé, halteres ao lado do corpo. Dê um passo à frente flexionando os dois joelhos a 90°. O joelho de trás quase toca o chão. Impulsione para frente com a perna dianteira, passando a outra perna à frente.",
            "Mantenha coluna ereta. Apoie no calcanhar da frente.",
            "Afundo andando unilateral para pernas.", "unilateral,pernas,funcional",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary)
            },
            new[] { Equipment.Dumbbell },
            ExerciseModality.Functional);

        Add("Esteira (Caminhada ou Corrida)", "esteira", DifficultyLevel.Beginner, MeasurementType.DistanceAndTime,
            "Suba na esteira. Ajuste velocidade (caminhada 5-6 km/h ou corrida 9+ km/h) e inclinação desejada. Mantenha postura ereta, braços soltos. Execute pelo tempo/distância definidos.",
            "Comece devagar, aqueça 5 min. Use tênis apropriado.",
            "Cardio clássico na esteira.", "cardio,esteira,caminhada,corrida",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Primary),
                (Muscle.Calves, MuscleRole.Secondary),
                (Muscle.Glutes, MuscleRole.Secondary)
            },
            new[] { Equipment.Treadmill },
            ExerciseModality.Cardio);

        Add("Bicicleta Ergométrica", "bicicleta-ergometrica", DifficultyLevel.Beginner, MeasurementType.DistanceAndTime,
            "Sente-se e ajuste a altura do selim (joelho quase estendido no ponto mais baixo). Ajuste a resistência. Pedale mantendo cadência 60-90 rpm pelo tempo/distância desejados.",
            "Mantenha os joelhos alinhados, não abra ou feche excessivamente.",
            "Cardio com baixo impacto nas articulações.", "cardio,bike,ciclismo",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary),
                (Muscle.Calves, MuscleRole.Secondary)
            },
            new[] { Equipment.Bike, Equipment.StationaryBike },
            ExerciseModality.Cardio);

        Add("Remo Ergômetro", "remo-ergometro", DifficultyLevel.Intermediate, MeasurementType.DistanceAndTime,
            "Sente-se no ergômetro, pés firmes. Posição inicial: joelhos flexionados, braços estendidos, tronco levemente à frente. Empurre com as pernas, abra o quadril, puxe a manopla até o peito. Volte na ordem inversa.",
            "Use as pernas primeiro, depois core e só depois braços.",
            "Cardio completo com força nas pernas e costas.", "cardio,remo,total-body",
            new[]
            {
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Back, MuscleRole.Primary),
                (Muscle.Hamstrings, MuscleRole.Secondary),
                (Muscle.Biceps, MuscleRole.Secondary)
            },
            new[] { Equipment.RowingMachine, Equipment.Rower },
            ExerciseModality.Cardio);

        Add("Burpees", "burpees", DifficultyLevel.Intermediate, MeasurementType.Repetitions,
            "Em pé. Agache, coloque as mãos no chão, pule os pés para trás ficando em posição de prancha. Faça uma flexão de braços. Pule os pés de volta às mãos. Salte para cima com os braços estendidos.",
            "Modificação: pule a flexão ou faça de joelhos.",
            "Exercício funcional de alta intensidade.", "funcional,hiit,fullbody",
            new[]
            {
                (Muscle.Chest, MuscleRole.Primary),
                (Muscle.Quads, MuscleRole.Primary),
                (Muscle.Glutes, MuscleRole.Primary),
                (Muscle.Triceps, MuscleRole.Secondary),
                (Muscle.Abs, MuscleRole.Stabilizer)
            },
            new[] { Equipment.Bodyweight },
            ExerciseModality.Functional);

        Add("Pular Corda", "pular-corda", DifficultyLevel.Beginner, MeasurementType.Time,
            "Pegue a corda, ajuste o comprimento (pés no meio da corda, manoplas chegando às axilas). Pule com pequenos saltos usando pontas dos pés. Gire a corda com os pulsos, não com os braços.",
            "Use tênis com amortecimento. Comece devagar, ganhe ritmo.",
            "Cardio divertido e eficiente para queima calórica.", "cardio, corda, agilidade",
            new[]
            {
                (Muscle.Calves, MuscleRole.Primary),
                (Muscle.Quads, MuscleRole.Secondary),
                (Muscle.Forearms, MuscleRole.Secondary),
                (Muscle.Shoulders, MuscleRole.Secondary)
            },
            new[] { Equipment.Bodyweight },
            ExerciseModality.Functional);

        return list;
    }
}

public class AdminSeedOptions
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
