export type Theme = {
  name: string;
  label: string;
  type: "light" | "dark";
  colors: {
    background: string;
    foreground: string;
    card: string;
    "card-foreground": string;
    popover: string;
    "popover-foreground": string;
    primary: string;
    "primary-foreground": string;
    secondary: string;
    "secondary-foreground": string;
    muted: string;
    "muted-foreground": string;
    accent: string;
    "accent-foreground": string;
    destructive: string;
    "destructive-foreground": string;
    border: string;
    input: string;
    ring: string;
    radius: string;
    "chart-1": string;
    "chart-2": string;
    "chart-3": string;
    "chart-4": string;
    "chart-5": string;
    "sidebar-background": string;
    "sidebar-foreground": string;
    "sidebar-primary": string;
    "sidebar-primary-foreground": string;
    "sidebar-accent": string;
    "sidebar-accent-foreground": string;
    "sidebar-border": string;
    "sidebar-ring": string;
  };
};

const sharedColors = {
  // Default sidebar colors if not specified
  sidebar: {
    background: "#0F172A",
    foreground: "#F8FAFC",
    border: "#1E293B",
    accent: "#1E293B",
    "accent-foreground": "#F8FAFC",
    primary: "#3B82F6",
    "primary-foreground": "#FFFFFF",
    ring: "#3B82F6",
  }
}

export const themes: Record<string, Theme> = {
  "curiosity-loop": {
    name: "curiosity-loop",
    label: "Curiosity Loop",
    type: "dark",
    colors: {
      background: "#020617",
      foreground: "#F1F5F9",
      card: "#020617",
      "card-foreground": "#F1F5F9",
      popover: "#020617",
      "popover-foreground": "#F1F5F9",
      primary: "#38BDF8",
      "primary-foreground": "#020617",
      secondary: "#6366F1",
      "secondary-foreground": "#F1F5F9",
      muted: "#1e293b",
      "muted-foreground": "#94A3B8",
      accent: "#F43F5E",
      "accent-foreground": "#F1F5F9",
      destructive: "#ef4444",
      "destructive-foreground": "#F1F5F9",
      border: "#1e293b",
      input: "#1e293b",
      ring: "#38BDF8",
      radius: "0.75rem",
      "chart-1": "#38BDF8",
      "chart-2": "#6366F1",
      "chart-3": "#F43F5E",
      "chart-4": "#22C55E",
      "chart-5": "#F59E0B",
      "sidebar-background": "#020617",
      "sidebar-foreground": "#F1F5F9",
      "sidebar-primary": "#38BDF8",
      "sidebar-primary-foreground": "#020617",
      "sidebar-accent": "#1E293B",
      "sidebar-accent-foreground": "#F1F5F9",
      "sidebar-border": "#1E293B",
      "sidebar-ring": "#38BDF8",
    },
  },
  "derot-brain": {
    name: "derot-brain",
    label: "Derot Brain",
    type: "dark",
    colors: {
      background: "#0B0E14",
      foreground: "#E5E7EB",
      card: "#161A22",
      "card-foreground": "#E5E7EB",
      popover: "#161A22",
      "popover-foreground": "#E5E7EB",
      primary: "#7C3AED",
      "primary-foreground": "#FFFFFF",
      secondary: "#A78BFA",
      "secondary-foreground": "#0B0E14",
      muted: "#1f2937",
      "muted-foreground": "#9CA3AF",
      accent: "#22C55E",
      "accent-foreground": "#0B0E14",
      destructive: "#ef4444",
      "destructive-foreground": "#FFFFFF",
      border: "#1f2937",
      input: "#1f2937",
      ring: "#7C3AED",
      radius: "0.5rem",
      "chart-1": "#7C3AED",
      "chart-2": "#22C55E",
      "chart-3": "#F59E0B",
      "chart-4": "#A78BFA",
      "chart-5": "#EF4444",
      "sidebar-background": "#0B0E14",
      "sidebar-foreground": "#E5E7EB",
      "sidebar-primary": "#7C3AED",
      "sidebar-primary-foreground": "#FFFFFF",
      "sidebar-accent": "#1F2937",
      "sidebar-accent-foreground": "#E5E7EB",
      "sidebar-border": "#1F2937",
      "sidebar-ring": "#7C3AED",
    },
  },
  "knowledge-core": {
    name: "knowledge-core",
    label: "Knowledge Core",
    type: "dark",
    colors: {
      background: "#0F172A",
      foreground: "#F8FAFC",
      card: "#1E293B",
      "card-foreground": "#F8FAFC",
      popover: "#1E293B",
      "popover-foreground": "#F8FAFC",
      primary: "#14C8F6",
      "primary-foreground": "#0F172A",
      secondary: "#64748B",
      "secondary-foreground": "#F8FAFC",
      muted: "#334155",
      "muted-foreground": "#CBD5E1",
      accent: "#22D3EE",
      "accent-foreground": "#0F172A",
      destructive: "#ef4444",
      "destructive-foreground": "#F8FAFC",
      border: "#334155",
      input: "#334155",
      ring: "#3B82F6",
      radius: "0.25rem",
      "chart-1": "#3B82F6",
      "chart-2": "#22D3EE",
      "chart-3": "#64748B",
      "chart-4": "#10B981",
      "chart-5": "#F59E0B",
      "sidebar-background": "#0F172A",
      "sidebar-foreground": "#F8FAFC",
      "sidebar-primary": "#14C8F6",
      "sidebar-primary-foreground": "#0F172A",
      "sidebar-accent": "#334155",
      "sidebar-accent-foreground": "#F8FAFC",
      "sidebar-border": "#334155",
      "sidebar-ring": "#3B82F6",
    },
  },
  "mind-lab": {
    name: "mind-lab",
    label: "Mind Lab",
    type: "dark",
    colors: {
      background: "#0F172A",
      foreground: "#E2E8F0",
      card: "#020617",
      "card-foreground": "#E2E8F0",
      popover: "#020617",
      "popover-foreground": "#E2E8F0",
      primary: "#14B8A6",
      "primary-foreground": "#0F172A",
      secondary: "#0EA5E9",
      "secondary-foreground": "#E2E8F0",
      muted: "#1e293b",
      "muted-foreground": "#94A3B8",
      accent: "#FACC15",
      "accent-foreground": "#0F172A",
      destructive: "#ef4444",
      "destructive-foreground": "#FFFFFF",
      border: "#1e293b",
      input: "#1e293b",
      ring: "#14B8A6",
      radius: "1rem", // very rounded for "lab"
      "chart-1": "#14B8A6",
      "chart-2": "#0EA5E9",
      "chart-3": "#FACC15",
      "chart-4": "#F97316",
      "chart-5": "#EC4899",
      "sidebar-background": "#0F172A",
      "sidebar-foreground": "#E2E8F0",
      "sidebar-primary": "#14B8A6",
      "sidebar-primary-foreground": "#0F172A",
      "sidebar-accent": "#1E293B",
      "sidebar-accent-foreground": "#E2E8F0",
      "sidebar-border": "#1E293B",
      "sidebar-ring": "#14B8A6",
    },
  },
  "neo-wikipedia": {
    name: "neo-wikipedia",
    label: "Neo-Wikipedia",
    type: "light",
    colors: {
      background: "#FAFAF9",
      foreground: "#111827",
      card: "#FFFFFF",
      "card-foreground": "#111827",
      popover: "#FFFFFF",
      "popover-foreground": "#111827",
      primary: "#2563EB",
      "primary-foreground": "#FFFFFF",
      secondary: "#475569",
      "secondary-foreground": "#FAFAF9",
      muted: "#F3F4F6",
      "muted-foreground": "#374151",
      accent: "#16A34A",
      "accent-foreground": "#FFFFFF",
      destructive: "#ef4444",
      "destructive-foreground": "#FFFFFF",
      border: "#E5E7EB",
      input: "#E5E7EB",
      ring: "#2563EB",
      radius: "0rem", // Square for wikipedia look
      "chart-1": "#2563EB",
      "chart-2": "#16A34A",
      "chart-3": "#475569",
      "chart-4": "#F59E0B",
      "chart-5": "#EF4444",
      "sidebar-background": "#F3F4F6",
      "sidebar-foreground": "#111827",
      "sidebar-primary": "#2563EB",
      "sidebar-primary-foreground": "#FFFFFF",
      "sidebar-accent": "#E5E7EB",
      "sidebar-accent-foreground": "#111827",
      "sidebar-border": "#E5E7EB",
      "sidebar-ring": "#2563EB",
    },
  },
};

export const defaultTheme = themes["derot-brain"];
