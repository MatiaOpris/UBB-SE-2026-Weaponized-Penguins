-- bun cred

USE Communities;
GO

-- =========================================================================
-- PART 1: WIPE EXISTING DATA (Child to Parent to avoid Foreign Key errors)
-- =========================================================================
DELETE FROM Notifications;
DELETE FROM CommentsViews;
DELETE FROM Comments;
DELETE FROM PostsViews;
DELETE FROM PostTags;
DELETE FROM Posts;
DELETE FROM UsersBets;
DELETE FROM Bets;
DELETE FROM USersMoodScores;
DELETE FROM CommunitiesUsers;
DELETE FROM Communities;
DELETE FROM UsersTokens;
DELETE FROM Tags;
DELETE FROM Categories;
DELETE FROM Users;

-- Reset Identity counters
DBCC CHECKIDENT ('Notifications', RESEED, 0);
DBCC CHECKIDENT ('Comments', RESEED, 0);
DBCC CHECKIDENT ('Posts', RESEED, 0);
DBCC CHECKIDENT ('Bets', RESEED, 0);
DBCC CHECKIDENT ('Communities', RESEED, 0);
DBCC CHECKIDENT ('Tags', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
GO

-- =========================================================================
-- PART 2: FOUNDATIONAL DATA
-- =========================================================================

-- 1. Insert 15 Users (USER 1 IS YOUR TEST ACCOUNT)
INSERT INTO Users (username, email, passwordHash, bio, status) VALUES
('@AlexB', 'alex@boards.wp', 'f4efa8dc99f406a97b9f8644eb841eb5becd5bd883e5fbd97b84f28cae9d3610', 'Dev & Gamer', 'Online'),
('@SarahC', 'sarah@test.com', 'hash2', 'Artist and Designer', 'Offline'),
('@MikeD', 'mike@test.com', 'hash3', 'Sports Fanatic', 'Online'),
('@ElenaT', 'elena@test.com', 'hash4', 'Chef and Foodie', 'Away'),
('@DavidW', 'david@test.com', 'hash5', 'Music Producer', 'In Game'),
('@DrSmith', 'smith@test.com', 'hash6', 'Physics Researcher', 'Online'),
('@HistoryBuff', 'history@test.com', 'hash7', 'Ancient Rome Enthusiast', 'Offline'),
('@Wanderlust', 'wander@test.com', 'hash8', 'Backpacking the world', 'Online'),
('@Cinephile', 'cinema@test.com', 'hash9', 'Movie Critic', 'Away'),
('@CodeNinja', 'ninja@test.com', 'hash10', 'C# Master', 'Online'),
('@ProGamer', 'progamer@test.com', 'hash11', 'Esports Champion', 'In Game'),
('@VeganLife', 'vegan@test.com', 'hash12', 'Plant based living', 'Online'),
('@GuitarHero', 'guitar@test.com', 'hash13', 'Jazz Musician', 'Offline'),
('@SpaceNerd', 'space@test.com', 'hash14', 'Looking at the stars', 'Online'),
('@TravelBug', 'bug@test.com', 'hash15', 'Next stop: Japan', 'Online');

-- 2. Insert the exact 24 requested Categories with specific UI colors
INSERT INTO Categories (categoryName, categoryColor) VALUES
('Arts & Design', '#FF69B4'),                 -- 1 Pink
('Music & Audio', '#FF69B4'),                 -- 2 Pink
('Literature & Writing', '#FF69B4'),          -- 3 Pink
('Internet Culture & Humor', '#E67E22'),      -- 4 Orange
('Food & Gastronomy', '#E67E22'),             -- 5 Orange
('Gaming', '#E67E22'),                        -- 6 Orange
('Physical Sciences & Space', '#1ABC9C'),     -- 7 Turquoise
('Technology & Computing', '#1ABC9C'),        -- 8 Turquoise
('Engineering & Systems', '#1ABC9C'),         -- 9 Turquoise
('Animals & Wildlife', '#F1C40F'),            -- 10 Yellow
('Environment & Ecology', '#F1C40F'),         -- 11 Yellow
('Travel & Geography', '#F1C40F'),            -- 12 Yellow
('Politics & Policy', '#3498DB'),             -- 13 Blue
('Law & Justice', '#3498DB'),                 -- 14 Blue
('Economics & Business', '#3498DB'),          -- 15 Blue
('Philosophy & Spirituality', '#2ECC71'),     -- 16 Green
('Health & Wellness', '#2ECC71'),             -- 17 Green
('Sports & Athletics', '#2ECC71'),            -- 18 Green
('Social & Relationships', '#E74C3C'),        -- 19 Red
('Lifestyle & Home', '#E74C3C'),              -- 20 Red
('Fashion & Grooming', '#E74C3C'),            -- 21 Red
('Education', '#9B59B6'),                     -- 22 Purple
('History & Culture', '#9B59B6'),             -- 23 Purple
('Hobbies & Leisure', '#9B59B6');             -- 24 Purple

-- 3. Insert 96 Tags (4 per Category to map perfectly)
INSERT INTO Tags (tagCategoryID, tagName) VALUES
(1, 'Digital Art'), (1, 'Sketching'), (1, '3D Modeling'), (1, 'Painting'),
(2, 'Rock'), (2, 'Jazz'), (2, 'EDM'), (2, 'Classical'),
(3, 'Novels'), (3, 'Poetry'), (3, 'Writing'), (3, 'Books'),
(4, 'Memes'), (4, 'Viral'), (4, 'Trends'), (4, 'Humor'),
(5, 'Vegan'), (5, 'Baking'), (5, 'BBQ'), (5, 'Recipes'),
(6, 'RPG'), (6, 'FPS'), (6, 'Esports'), (6, 'Console'),
(7, 'Physics'), (7, 'Astronomy'), (7, 'Biology'), (7, 'Quantum'),
(8, 'AI'), (8, 'WinUI'), (8, 'C#'), (8, 'Cloud'),
(9, 'Mechanical'), (9, 'Electrical'), (9, 'Civil'), (9, 'Robotics'),
(10, 'Pets'), (10, 'Wildlife'), (10, 'Ocean'), (10, 'Birds'),
(11, 'Climate'), (11, 'Conservation'), (11, 'Renewable'), (11, 'Nature'),
(12, 'Backpacking'), (12, 'Europe'), (12, 'Asia'), (12, 'Luxury'),
(13, 'Elections'), (13, 'Policy'), (13, 'Global'), (13, 'Local'),
(14, 'Courts'), (14, 'Rights'), (14, 'Crime'), (14, 'LegalAdvice'),
(15, 'Stocks'), (15, 'Startups'), (15, 'Crypto'), (15, 'Markets'),
(16, 'Ethics'), (16, 'Meditation'), (16, 'Religion'), (16, 'Logic'),
(17, 'Fitness'), (17, 'MentalHealth'), (17, 'Nutrition'), (17, 'Sleep'),
(18, 'Football'), (18, 'Basketball'), (18, 'F1'), (18, 'Tennis'),
(19, 'Dating'), (19, 'Friends'), (19, 'Family'), (19, 'Advice'),
(20, 'Decor'), (20, 'DIY'), (20, 'Gardening'), (20, 'Organization'),
(21, 'Streetwear'), (21, 'Skincare'), (21, 'Hair'), (21, 'Vintage'),
(22, 'College'), (22, 'Study'), (22, 'Teaching'), (22, 'Courses'),
(23, 'Ancient'), (23, 'Medieval'), (23, 'WW2'), (23, 'Rome'),
(24, 'Crafts'), (24, 'BoardGames'), (24, 'Woodworking'), (24, 'Collecting');

-- 4. Users Tokens
INSERT INTO UsersTokens (userID, tokens)
SELECT userID, (userID * 100) + 50 FROM Users;

-- 5. Insert 24 Communities (1:1 with Categories)
INSERT INTO Communities (name, description, adminID, membersNumber) VALUES
('Canvas & Pixels', 'Share your artwork and get feedback.', 1, 15),
('Soundwaves', 'Music production, listening, and sharing.', 2, 15),
('The Library', 'Discuss books, writing, and literature.', 3, 15),
('Meme Central', 'The front page of internet culture.', 4, 15),
('Culinary Masters', 'Recipes, techniques, and food pics.', 5, 15),
('The Gamer''s Nexus', 'Discuss all things gaming.', 6, 15),
('Science Lab', 'Discussing the latest in physical sciences.', 7, 15),
('DevHub', 'Software engineering and technology.', 8, 15),
('The Blueprint', 'Engineering, systems, and robotics.', 9, 15),
('The Wild', 'Everything about animals and wildlife.', 10, 15),
('Eco Warriors', 'Climate, nature, and conservation.', 11, 15),
('Global Nomads', 'Travel tips, itineraries, and stories.', 12, 15),
('The Capitol', 'Politics and global policy discussions.', 13, 15),
('Legal Eagles', 'Law, justice, and legal frameworks.', 14, 15),
('Wall Street', 'Economics, markets, and business.', 15, 15),
('The Think Tank', 'Philosophy, ethics, and spirituality.', 1, 15),
('Mind & Body', 'Health, wellness, and fitness.', 2, 15),
('The Stadium', 'Live match threads and sports debate.', 3, 15),
('The Lounge', 'Socializing and relationship advice.', 4, 15),
('Better Homes', 'Lifestyle, DIY, and home improvement.', 5, 15),
('The Runway', 'Fashion, grooming, and style trends.', 6, 15),
('The Academy', 'Education, courses, and studying.', 7, 15),
('Time Travelers', 'Exploring history and culture.', 8, 15),
('The Workshop', 'Hobbies, crafts, and leisure activities.', 9, 15);

-- 6. Connect ALL users to ALL communities for massive testing data
INSERT INTO CommunitiesUsers (communityID, userID)
SELECT c.communityID, u.userID FROM Communities c CROSS JOIN Users u;

-- 7. Add Base Mood Scores
INSERT INTO USersMoodScores (userID, categoryID, score)
SELECT u.userID, c.categoryID, (u.userID * 10) + (c.categoryID * 5)
FROM Users u CROSS JOIN Categories c WHERE u.userID <= 5;
GO

-- =========================================================================
-- PART 3: GENERATE HISTORICAL DIVERSE POSTS (Ages 0 to 6 days old)
-- =========================================================================

CREATE TABLE #BasePosts (commID INT, title NVARCHAR(100), descr NVARCHAR(3000));
INSERT INTO #BasePosts VALUES
(1, 'My first digital portrait', 'Used Procreate and took about 8 hours. Critiques are welcome!'),
(2, 'Top 10 guitar solos of the 90s', 'Nothing beats Pearl Jam or Nirvana in my opinion. What is yours?'),
(3, 'How to overcome writer''s block', 'Staring at a blank page for 3 hours now...'),
(4, 'The meme cycle is getting faster', 'Memes die in literally 2 days now.'),
(5, 'Best sourdough starter tips', 'Mine keeps dying after a week. What temperature should I keep it at?'),
(6, 'Best loadout for Season 5?', 'I feel like the assault rifles got nerfed. What are you guys using?'),
(7, 'James Webb Telescope new images', 'The clarity on these nebulae is mind-blowing.'),
(8, 'C# 12 Features you must know', 'Primary constructors are an absolute game changer.'),
(9, 'The future of Boston Dynamics', 'Their new robot is terrifyingly agile.'),
(10, 'Adopted a stray cat today', 'She is so cute! Name suggestions?'),
(11, 'The ocean cleanup project', 'They just removed a record amount of plastic.'),
(12, 'Backpacking through Southeast Asia', 'Budget tips for Vietnam and Thailand. Is $30 a day enough?'),
(13, 'Global trade agreements impact', 'Tariffs are affecting local businesses.'),
(14, 'What exactly is fair use?', 'Copyright strikes on YouTube are out of hand.'),
(15, 'Is the S&P 500 overvalued?', 'Tech stocks are carrying the entire market.'),
(16, 'Stoicism in modern life', 'Applying Marcus Aurelius to corporate jobs.'),
(17, 'Best pre-workout supplements', 'Looking for something without caffeine.'),
(18, 'Champions League final predictions', 'Who is taking home the trophy this year?'),
(19, 'How to make friends in your 30s?', 'It feels impossible outside of work.'),
(20, 'Spring cleaning checklist', 'Don''t forget to clean your washing machine filter.'),
(21, 'Building a capsule wardrobe', 'What are your 5 essential pieces?'),
(22, 'The Pomodoro technique changed my life', 'I actually got my thesis done.'),
(23, 'Fall of the Roman Empire', 'Was it inevitable due to their massive expansion?'),
(24, 'Getting into miniature painting', 'What brushes do you recommend for beginners?');

-- Generate 240 posts. Subtracts random days (0 to 6) to test historical sorts!
DECLARE @i INT = 1;
WHILE @i <= 10
BEGIN
    INSERT INTO Posts (ownerID, communityID, title, description, score, commentsNumber, creationTime)
    SELECT 
        (ABS(CHECKSUM(NEWID())) % 15) + 1, 
        commID,
        CASE @i 
            WHEN 1 THEN title 
            WHEN 2 THEN 'Update: ' + title 
            WHEN 3 THEN 'Thoughts on: ' + title 
            WHEN 4 THEN 'Re: ' + title 
            ELSE 'Discussion: ' + title 
        END,
        descr + ' Let me know what you think below!',
        (ABS(CHECKSUM(NEWID())) % 500), 
        2, 
        -- Randomly subtract 0 to 6 days from today
        DATEADD(day, -(ABS(CHECKSUM(NEWID())) % 7), GETDATE())
    FROM #BasePosts;
    
    SET @i = @i + 1;
END
DROP TABLE #BasePosts;
GO

-- =========================================================================
-- PART 4: GENERATE ASSOCIATED DATA (Tags, Views, Comments)
-- =========================================================================

-- 1. Mass Insert PostTags (Connects posts to the exact tags of their community's category)
INSERT INTO PostTags (postID, tagID, position)
SELECT postID, ((communityID - 1) * 4) + 1, 0 FROM Posts;

INSERT INTO PostTags (postID, tagID, position)
SELECT postID, ((communityID - 1) * 4) + 2, 1 FROM Posts;

INSERT INTO PostTags (postID, tagID, position)
SELECT postID, ((communityID - 1) * 4) + 3, 2 FROM Posts;

INSERT INTO PostTags (postID, tagID, position)
SELECT postID, ((communityID - 1) * 4) + 4, 3 FROM Posts;

-- 2. Simulate random likes/dislikes
INSERT INTO PostsViews (userID, postID, vote)
SELECT (postID % 15) + 1, postID, CASE WHEN postID % 3 = 0 THEN 2 ELSE 1 END FROM Posts;

-- 3. Insert Comments
INSERT INTO Comments (postID, ownerID, description, score, indentation)
SELECT postID, (ABS(CHECKSUM(NEWID())) % 15) + 1, 'This is a great perspective on the topic!', (ABS(CHECKSUM(NEWID())) % 50), 0
FROM Posts;

INSERT INTO Comments (postID, ownerID, description, score, indentation)
SELECT postID, (ABS(CHECKSUM(NEWID())) % 15) + 1, 'Interesting take, but I have to disagree.', (ABS(CHECKSUM(NEWID())) % 20), 0
FROM Posts;

-- 4. Insert Notifications
INSERT INTO Notifications (postID, receiverID, actorID, actionType, isRead)
SELECT p.postID, p.ownerID, c.ownerID, 1, CASE WHEN c.commentID % 4 = 0 THEN 1 ELSE 0 END 
FROM Posts p JOIN Comments c ON p.postID = c.postID WHERE p.ownerID != c.ownerID; 

-- =========================================================================
-- PART 5: BETS & WAGERS
-- =========================================================================

INSERT INTO Bets (communityID, betType, startingTime, endingTime, expression, yesAmount, notAmount) VALUES
(6, 1, GETDATE(), DATEADD(day, 7, GETDATE()), 'Will GTA VI drop in 2026?', 1500, 500),
(8, 1, GETDATE(), DATEADD(day, 14, GETDATE()), 'Will AI write 80% of code by 2030?', 2000, 1000),
(18, 2, GETDATE(), DATEADD(day, 3, GETDATE()), 'Will the underdogs win the finals?', 800, 1200);

INSERT INTO UsersBets (userID, betID, amount, odd, betVote) VALUES
(1, 1, 100, 1.5, 1),
(2, 1, 50, 2.0, 0),
(10, 2, 500, 1.2, 1),
(3, 3, 200, 2.5, 1);
GO