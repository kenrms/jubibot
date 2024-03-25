﻿// <auto-generated />
using DiscordBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiscordBot.Migrations
{
    [DbContext(typeof(BotContext))]
    partial class BotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("DiscordBot.Models.BotConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("OpenAiMaxTokens")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpenAiModel")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OpenAiSystemPrompt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<float>("OpenAiTemperature")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("BotConfigurations");
                });
#pragma warning restore 612, 618
        }
    }
}
