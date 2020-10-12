#Create Database
Create database Birthday_DiscordBot;
use Birthday_DiscordBot;

#Create Table Guilds
CREATE TABLE guilds (
    guildID BIGINT UNSIGNED PRIMARY KEY,
    channelID BIGINT UNSIGNED,
    prefix VARCHAR(5) default "!"
);

#Create Table user
create table user(
	ID int primary key auto_increment,
    userID bigint unsigned,
    birthday date,
    guild bigint unsigned,
    foreign key (guild) references guilds(guildID) on update cascade on delete cascade 
);

#Query through user table
select * from user;
select * from guilds;
#describe user;

#select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = 484297678159609856 and user.birthday rlike '^CU[0-9]'+'-10-20';
#select * from user left join guilds on user.guild = guilds.ID;


#drop database Birthday_DiscordBot;