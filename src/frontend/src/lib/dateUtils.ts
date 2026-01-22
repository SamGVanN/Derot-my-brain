export const parseDate = (dateString: string): Date => {
    if (!dateString) return new Date('Invalid');

    // Try parsing directly first
    let date = new Date(dateString);
    if (!isNaN(date.getTime())) return date;

    // Handle SQL format "YYYY-MM-DD HH:MM:SS" which some browsers/libs struggle with
    // by replacing the space with T
    const isoString = dateString.replace(' ', 'T');
    date = new Date(isoString);
    if (!isNaN(date.getTime())) return date;

    // If it's just a date "YYYY-MM-DD", it should have worked, but let's try ensuring T00:00:00
    if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
        return new Date(`${dateString}T00:00:00`);
    }

    return new Date('Invalid');
};

export const isValidDate = (date: Date): boolean => {
    return !isNaN(date.getTime());
};
