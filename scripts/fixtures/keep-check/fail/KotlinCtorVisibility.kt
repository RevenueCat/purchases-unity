package com.revenuecat.fixtures

// The private modifier applies to the constructor, not the class: still a public type.
// Expected flagged: CtorVisibility
class CtorVisibility private constructor()
