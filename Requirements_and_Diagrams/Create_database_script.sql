CREATE DATABASE Communities;
GO
USE Communities;
GO


-- 1. Users Table 
CREATE TABLE Users (
    userID INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) NOT NULL UNIQUE,
    passwordHash NVARCHAR(255) NOT NULL,
    avatarUrl VARBINARY(MAX),
    bio NVARCHAR(MAX),
    status NVARCHAR(50)
);


-- 2. Categories Table
CREATE TABLE Categories (
    categoryID INT IDENTITY(1,1) PRIMARY KEY,
    categoryName NVARCHAR(50) NOT NULL UNIQUE,
    categoryColor VARCHAR(7) NOT NULL 
);


-- 3. Tags Table
CREATE TABLE Tags (
    tagID INT IDENTITY(1,1) PRIMARY KEY,
    tagCategoryID INT NOT NULL,
    tagName NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Tags_Categories FOREIGN KEY (tagCategoryID) REFERENCES Categories(categoryID)
);


-- 4. UsersTokens Table
CREATE TABLE UsersTokens (
    userID INT PRIMARY KEY,
    tokens INT DEFAULT 5 NOT NULL, 
    lastSeen DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_UsersTokens_Users FOREIGN KEY (userID) REFERENCES Users(userID)
);


-- 5. Communities Table
CREATE TABLE Communities (
    communityID INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL UNIQUE,
    description NVARCHAR(500) NOT NULL,
    picture VARBINARY(MAX),
    banner VARBINARY(MAX),
    membersNumber INT DEFAULT 1,
    adminID INT NOT NULL,
    CONSTRAINT FK_Communities_Users FOREIGN KEY (adminID) REFERENCES Users(userID)
);


-- 6. CommunitiesUsers Table
CREATE TABLE CommunitiesUsers (
    communityID INT NOT NULL,
    userID INT NOT NULL,
    PRIMARY KEY (communityID, userID),
    CONSTRAINT FK_CommunitiesUsers_Communities FOREIGN KEY (communityID) REFERENCES Communities(communityID) ON DELETE CASCADE,
    CONSTRAINT FK_CommunitiesUsers_Users FOREIGN KEY (userID) REFERENCES Users(userID) -- NO ACTION to avoid cascade paths
);


-- 7. Bets Table
CREATE TABLE Bets (
    betID INT IDENTITY(1,1) PRIMARY KEY,
    communityID INT NOT NULL,
    betType INT NOT NULL,
    startingTime DATETIME NOT NULL,
    endingTime DATETIME NOT NULL,
    expression NVARCHAR(50) NOT NULL, 
    yesAmount INT DEFAULT 0,
    notAmount INT DEFAULT 0,
    CONSTRAINT FK_Bets_Communities FOREIGN KEY (communityID) REFERENCES Communities(communityID) ON DELETE CASCADE,
    CONSTRAINT CHK_ExpressionLength CHECK (LEN(expression) >= 3 AND LEN(expression) <= 50)
);


-- 8. UsersBets Table
CREATE TABLE UsersBets (
    userID INT NOT NULL,
    betID INT NOT NULL,
    amount INT NOT NULL,
    odd DECIMAL(10,2) NOT NULL,
    betVote INT NOT NULL,
    PRIMARY KEY (userID, betID),
    CONSTRAINT FK_UsersBets_Users FOREIGN KEY (userID) REFERENCES UsersTokens(userID),
    CONSTRAINT FK_UsersBets_Bets FOREIGN KEY (betID) REFERENCES Bets(betID) ON DELETE CASCADE
);


-- 9. USersMoodScores Table
CREATE TABLE USersMoodScores (
    userID INT NOT NULL,
    categoryID INT NOT NULL,
    score INT NOT NULL,
    PRIMARY KEY (userID, categoryID),
    CONSTRAINT FK_USersMoodScores_Users FOREIGN KEY (userID) REFERENCES Users(userID) ON DELETE CASCADE,
    CONSTRAINT FK_USersMoodScores_Categories FOREIGN KEY (categoryID) REFERENCES Categories(categoryID) ON DELETE CASCADE
);


-- 10. Posts Table
CREATE TABLE Posts (
    postID INT IDENTITY(1,1) PRIMARY KEY,
    ownerID INT NOT NULL,
    communityID INT NOT NULL,
    title NVARCHAR(100) NOT NULL, 
    description NVARCHAR(3000) NOT NULL, 
    image VARBINARY(MAX),
    score INT DEFAULT 0,
    commentsNumber INT DEFAULT 0,
    creationTime DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Posts_Users FOREIGN KEY (ownerID) REFERENCES Users(userID),
    CONSTRAINT FK_Posts_Communities FOREIGN KEY (communityID) REFERENCES Communities(communityID) ON DELETE CASCADE
);


-- 11. PostTags Table
CREATE TABLE PostTags (
    postID INT NOT NULL,
    tagID INT NOT NULL, 
    position INT NOT NULL,
    PRIMARY KEY (postID, tagID),
    CONSTRAINT FK_PostTags_Posts FOREIGN KEY (postID) REFERENCES Posts(postID) ON DELETE CASCADE,
    CONSTRAINT FK_PostTags_Tags FOREIGN KEY (tagID) REFERENCES Tags(tagID) ON DELETE CASCADE,
    CONSTRAINT CHK_Position CHECK (position BETWEEN 0 AND 9)
);


-- 12. PostsViews Table
CREATE TABLE PostsViews (
    userID INT NOT NULL,
    postID INT NOT NULL,
    vote INT NOT NULL,
    PRIMARY KEY (userID, postID),
    CONSTRAINT FK_PostsViews_Users FOREIGN KEY (userID) REFERENCES Users(userID),
    CONSTRAINT FK_PostsViews_Posts FOREIGN KEY (postID) REFERENCES Posts(postID) ON DELETE CASCADE
);


-- 13. Comments Table
CREATE TABLE Comments (
    commentID INT IDENTITY(1,1) PRIMARY KEY,
    postID INT NOT NULL,
    parentID INT NULL,
    ownerID INT NOT NULL,
    description NVARCHAR(618) NOT NULL,
    score INT DEFAULT 0,
    creationTime DATETIME DEFAULT GETDATE(),
    indentation INT DEFAULT 0,
    isDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Comments_Posts FOREIGN KEY (postID) REFERENCES Posts(postID) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Parent FOREIGN KEY (parentID) REFERENCES Comments(commentID) NO ACTION,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (ownerID) REFERENCES Users(userID),
    CONSTRAINT CHK_Indentation CHECK (indentation BETWEEN 0 AND 7)
);


-- 14. CommentsViews Table
CREATE TABLE CommentsViews (
    userID INT NOT NULL,
    commentID INT NOT NULL,
    vote INT NOT NULL,
    PRIMARY KEY (userID, commentID),
    CONSTRAINT FK_CommentsViews_Users FOREIGN KEY (userID) REFERENCES Users(userID),
    CONSTRAINT FK_CommentsViews_Comments FOREIGN KEY (commentID) REFERENCES Comments(commentID) ON DELETE CASCADE
);


-- 15. Notifications Table
CREATE TABLE Notifications (
    notificationID INT IDENTITY(1,1) PRIMARY KEY,
    creationTime DATETIME DEFAULT GETDATE(),
    postID INT NULL,
    receiverID INT NOT NULL,
    actorID INT NOT NULL,
    actionType INT NOT NULL,
    isRead BIT DEFAULT 0,
    CONSTRAINT FK_Notifications_Posts FOREIGN KEY (postID) REFERENCES Posts(postID) ON DELETE CASCADE,
    CONSTRAINT FK_Notifications_Receiver FOREIGN KEY (receiverID) REFERENCES Users(userID),
    CONSTRAINT FK_Notifications_Actor FOREIGN KEY (actorID) REFERENCES Users(userID)
);