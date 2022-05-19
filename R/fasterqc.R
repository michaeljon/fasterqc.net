library(tidyverse)
library(jsonlite)
library(tidyjson)
library(ggplot2)
library(hrbrthemes)
library(tidyr)
library(stringr)
library(reshape2)
library(grid)

# Multiple plot function
#
# ggplot objects can be passed in ..., or to plotlist
# (as a list of ggplot objects)
# - cols:   Number of columns in layout
# - layout: A matrix specifying the layout. If present, 'cols' is ignored.
#
# If the layout is something like matrix(c(1,2,3,3), nrow=2, byrow=TRUE),
# then plot 1 will go in the upper left, 2 will go in the upper right, and
# 3 will go all the way across the bottom.
#
multiplot <- function(..., plotlist = NULL, file, cols = 1, layout = NULL) {
  # Make a list from the ... arguments and plotlist
  plots <- c(list(...), plotlist)

  num_plots <- length(plots)

  # If layout is NULL, then use 'cols' to determine layout
  if (is.null(layout)) {
    # Make the panel
    # ncol: Number of columns of plots
    # nrow: Number of rows needed, calculated from # of cols
    layout <- matrix(seq(1, cols * ceiling(num_plots / cols)),
      ncol = cols, nrow = ceiling(num_plots / cols)
    )
  }

  if (num_plots == 1) {
    print(plots[[1]])
  } else {
    # Set up the page
    grid.newpage()
    pushViewport(viewport(layout = grid.layout(nrow(layout), ncol(layout))))

    # Make each plot, in the correct location
    for (i in 1:num_plots) {
      # Get the i,j matrix positions of the regions that contain this subplot
      matchidx <- as.data.frame(which(layout == i, arr.ind = TRUE))

      print(plots[[i]], vp = viewport(
        layout.pos.row = matchidx$row,
        layout.pos.col = matchidx$col
      ))
    }
  }
}

setwd("/Users/michaeljon/src/ovation/fasterqc.net/tmp")
sample <- "in3257_2_S1_sorted"

# load the data
json <-
  jsonlite::fromJSON(readLines(paste(getwd(), "/", sample, ".json", sep = "")))

# plot the ACTGs
percent_actg <- data.frame(
  as = json$baseCounts$aPercentage,
  ts = json$baseCounts$tPercentage,
  cs = json$baseCounts$cPercentage,
  gs = json$baseCounts$gPercentage
)

plt <- ggplot(percent_actg, aes(x = seq_along(as))) +
  geom_line(aes(y = as, color = "%A")) +
  geom_line(aes(y = ts, color = "%T")) +
  geom_line(aes(y = cs, color = "%C")) +
  geom_line(aes(y = gs, color = "%G")) +
  coord_cartesian(ylim = c(0, 100)) +
  labs(
    color = "Nucleotide",
    title = "Percentage of ACTG by sequence position",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Position in read",
    y = "% of ACTG",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".pct-actg.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

# plot the N percentages
n_percentages <- data.frame(
  n = json$nPercentages$percentages
)

plt <- ggplot(n_percentages, aes(x = seq_along(n))) +
  geom_line(aes(y = n)) +
  coord_cartesian(ylim = c(0, 1)) +
  labs(
    title = "Percent of N reads",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Position in read",
    y = "% N",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".pct-n.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

# plot GC distribution
gc_distribution <- data.frame(
  gc = json$gcDistribution
)

plt <- ggplot(gc_distribution, aes(x = seq_along(gc))) +
  geom_line(aes(y = gc, color = "% GC")) +
  # geom_line(aes(y = cumsum(gc), color = "Cumulative")) +
  scale_y_continuous(labels = scales::comma) +
  labs(
    color = "",
    title = "GC distribution",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Position in read",
    y = "% GC",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".gc-dist.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

# plot quality distributions
qualities_by_base <- data.frame(
  as = json$qualityDistributionByBase$aDistribution,
  ts = json$qualityDistributionByBase$tDistribution,
  cs = json$qualityDistributionByBase$cDistribution,
  gs = json$qualityDistributionByBase$gDistribution
)

plt <- ggplot(qualities_by_base, aes(x = seq_along(as))) +
  geom_line(aes(y = as, color = "%A")) +
  geom_line(aes(y = ts, color = "%T")) +
  geom_line(aes(y = cs, color = "%C")) +
  geom_line(aes(y = gs, color = "%G")) +
  scale_y_continuous(labels = scales::comma, trans = "log10") +
  labs(
    color = "Nucleotide",
    title = "Quality distribution by base",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Quality (phred)",
    y = "# of sequences (log)",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".base-qual-dist.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

# plot quality distributions
quality_distribution <- data.frame(
  qual = json$meanQualityDistribution$distribution,
  low = json$meanQualityDistribution$lowestMean,
  high = json$meanQualityDistribution$highestMean
)

lowest_score <- min(quality_distribution$low)
highest_score <- max(quality_distribution$high)

plt <- ggplot(quality_distribution, aes(x = lowest_score:highest_score)) +
  geom_line(aes(y = qual)) +
  scale_y_continuous(labels = scales::comma, trans = "log10") +
  labs(
    title = "Mean quality distribution",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Quality (phred)",
    y = "# of sequences (log)",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".mean-qual-dist.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

sequence_length_distribution <- data.frame(
  len = json$sequenceLengthDistribution$distribution
)

plt <- ggplot(sequence_length_distribution, aes(x = seq_along(len))) +
  geom_line(aes(y = len)) +
  scale_y_continuous(labels = scales::comma, trans = "log10") +
  labs(
    title = "Read count by length",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Length of read",
    y = "# of sequences (log)",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".readlen.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

per_position_qualities <- data.frame(
  json$perPositionQual |> select(-distribution)
)

lowest_score <- min(per_position_qualities$lowestScore)
highest_score <- min(per_position_qualities$highestScore)

plt <- ggplot(per_position_qualities, aes(x = position)) +
  geom_line(aes(y = mean, color = "mean")) +
  geom_smooth(aes(y = lowest_score, color = "min"), level = 0.95) +
  geom_smooth(aes(y = highest_score, color = "max"), level = 0.95) +
  coord_cartesian(ylim = c(lowest_score, highest_score)) +
  scale_y_continuous(labels = scales::comma) +
  labs(
    color = "",
    title = "Read quality by position",
    subtitle = paste("Sample ", sample, sep = ""),
    x = "Position in read",
    y = "Phred score",
    caption = "Species: SARS-CoV-2",
  ) +
  theme_ipsum()

png(paste(sample, ".qual.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

histogram <- data.frame(
  paired = json$alignmentStatistics$histogram$paired,
  unpaired = json$alignmentStatistics$histogram$unpaired
)

minimum_read_length <- json$alignmentStatistics$histogram$minReadLength
maximum_read_length <- json$alignmentStatistics$histogram$maxReadLength

histogram$lengths <- minimum_read_length:maximum_read_length

plt <- ggplot(histogram, aes(x = lengths)) +
  geom_col(aes(y = paired), fill = "#ff7f7f") +
  scale_y_continuous(labels = scales::comma, trans = "log10") +
  coord_cartesian(xlim = c(minimum_read_length, maximum_read_length)) +
  labs(
    color = "Legend",
    title = "Paired segments",
    subtitle = "Sample in3257_2_S1",
    x = "Length of read",
    y = "Number of sequences (log)",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

png(paste(sample, ".paired.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()

plt <- ggplot(histogram, aes(x = lengths)) +
  geom_col(aes(y = unpaired), fill = "#7f7fff") +
  scale_y_continuous(labels = scales::comma, trans = "log10") +
  coord_cartesian(xlim = c(minimum_read_length, maximum_read_length)) +
  labs(
    color = "Legend",
    title = "Aligned and paired segments",
    subtitle = "Sample in3257_2_S1",
    x = "Length of read",
    y = "Number of sequences (log)",
    caption = "Species: SARS-CoV-2"
  ) +
  theme_ipsum()

# call multiplot(paired, unpaired) to put two of these on the page

png(paste(sample, ".aligned.png", sep = ""), width = 800, height = 600)
print(plt)
dev.off()