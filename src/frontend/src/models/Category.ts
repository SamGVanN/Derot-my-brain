export interface WikipediaCategory {
    id: string;
    name: string;
    nameFr: string;
    order: number;
    isActive: boolean;
}

export interface WikipediaCategoryList {
    categories: WikipediaCategory[];
}
